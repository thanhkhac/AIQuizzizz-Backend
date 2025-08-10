using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.AiGenerate.Services;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.AiGenerate;

[Authorize]
public class GenerateDocumentStructureCommand : IRequest<DocumentStructureDto>
{
    public required FileStreamData FileData { get; set; }
}

public class GenerateDocumentStructureCommandValidator : AbstractValidator<GenerateDocumentStructureCommand>
{
    private const long MaxFileSizeInBytes = 50 * 1024 * 1024; // 50MB

    public GenerateDocumentStructureCommandValidator()
    {
        RuleFor(x => x.FileData)
            .NotNull().WithMessage("FileData không được trống");

        RuleFor(x => x.FileData.Data)
            .NotNull().WithMessage("Dữ liệu stream không được trống")
            .Must(stream => stream!.Length > 0).WithMessage("Stream không được rỗng")
            .Must(stream => stream!.Length <= MaxFileSizeInBytes)
            .WithMessage("Dung lượng tệp không được vượt quá 50MB");
    }
}

public class GenerateDocumentStructureCommandHandler : IRequestHandler<GenerateDocumentStructureCommand, DocumentStructureDto>
{
    private readonly IAiGenerateService _aiGenerateService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public GenerateDocumentStructureCommandHandler(IAiGenerateService aiGenerateService, IApplicationDbContext context, IUser user)
    {
        _aiGenerateService = aiGenerateService;
        _context = context;
        _user = user;
    }

    public async Task<DocumentStructureDto> Handle(GenerateDocumentStructureCommand request, CancellationToken cancellationToken)
    {
        var uploadResult = await _aiGenerateService.UploadFileAsync(request.FileData, cancellationToken);
        var systemInstruction = PromptProvider.GetGenerateDocumentStructureSystemInstructionPrompt();
        var currentUser = await _context.DomainUsers.FindAsync(_user.UserId!.Value, cancellationToken);


        if (currentUser!.IsPaymentLocked == true)
            throw new ErrorCodeException(ErrorCodes.PAYMENT_IN_PROGRESS);

        currentUser.IsPaymentLocked = true;
        await _context.SaveChangesAsync(cancellationToken);
        try
        {
            //Đếm token của prompt
            var minimumExpectedTokens = await _aiGenerateService.CountTokenWithFileAsync(
                fileUri: uploadResult.FileUri,
                systemInstruction: systemInstruction,
                prompt: "",
                cancellationToken: cancellationToken);

            if (minimumExpectedTokens > SystemSettings.MaxInputToken)
                throw new ErrorCodeException(ErrorCodes.AI_FILE_TOO_LARGE);

            var apiInputCost = (double)minimumExpectedTokens / 1_000_000 * SystemSettings.InputCostPerMillionTokens;
            var apiMaxOutputCost = (double)SystemSettings.MaxOutputToken / 1_000_000 * SystemSettings.OutputCostPerMillionTokens;
            var minimumTotalPoint = (int)Math.Round(apiInputCost + apiMaxOutputCost + SystemSettings.FixedSystemFee);


            if (currentUser!.Balance < minimumTotalPoint)
                throw new ErrorCodeException(ErrorCodes.INSUFFICIENT_BALANCE,
                    $"minimumTotalPoint: {minimumTotalPoint}, tokenCount: {minimumExpectedTokens}");




            var result = await _aiGenerateService.SendPromptWithFileAsync(
                fileUri: uploadResult.FileUri,
                systemInstruction: systemInstruction,
                "",
                cancellationToken: cancellationToken);

            currentUser.Balance -= (int)Math.Round(apiInputCost + SystemSettings.FixedSystemFee);

            _context.DomainUsers.Update(currentUser);
            await _context.SaveChangesAsync(cancellationToken);

            if (result != "")
            {
                var outputToken = await _aiGenerateService.CountToken(
                    text1: "",
                    text2: result,
                    cancellationToken
                );

                var apiOutputCost = (double)outputToken / 1_000_000 * SystemSettings.OutputCostPerMillionTokens;
                var totalPoint = (int)Math.Round(apiOutputCost);
                currentUser.IsPaymentLocked = false;
                _context.DomainUsers.Update(currentUser);
                await _context.SaveChangesAsync(cancellationToken);
            }


            //Xử lý chuỗi
            int startIndex = result.IndexOf('{');
            int endIndex = result.LastIndexOf('}');

            string normalizedResult = (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
                ? result.Substring(startIndex, endIndex - startIndex + 1)
                : string.Empty;

            try
            {
                var jsonDoc = JsonDocument.Parse(normalizedResult);
                if (jsonDoc.RootElement.TryGetProperty("errorCode", out var errorCode))
                {
                    if (errorCode.GetString() == "NO_STRUCTURE_FOUND")
                    {
                        throw new ErrorCodeException(ErrorCodes.NO_STRUCTURE_FOUND);
                    }
                }
            }
            catch (JsonException)
            {
            }


            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var document = JsonSerializer.Deserialize<DocumentStructureDto>(normalizedResult, options);
                if (document == null)
                {
                    throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);
                }
                Console.WriteLine(JsonSerializer.Serialize(document, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                }));

                return document;
            }
            catch (Exception ex)
            {
                throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED, $"Lỗi giải mã JSON: {ex.Message}");
            }
        }
        catch (ErrorCodeException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED);
        }
        finally
        {
            currentUser!.IsPaymentLocked = false;
            _context.DomainUsers.Update(currentUser);
            await _context.SaveChangesAsync(cancellationToken);
            await _aiGenerateService.DeleteFileAsync(uploadResult.FileName);
        }
    }
}
