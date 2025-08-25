using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.AiGenerate.Services;
using CleanArchitectureBase.Application.Common.Atributes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.AiGenerate;

[Authorize]
public class GenerateQuestionWithAiCommand : IRequest<List<CreateUpdateQuestionDto>>
{
    [JsonIgnore]
    public required FileStreamData FileData { get; set; }
    public bool IsGenerateExplain { get; set; }
    public string? Language { get; set; }
    public int QuestionCount { get; set; }
    public List<string> QuestionTypes { get; set; } = new();
    
    [NoTrimRecursive]
    public DocumentStructureDto? DocumentStructure { get; set; }
    [NoTrimRecursive]
    public DocumentStructureDto? SelectedParts { get; set; }

}

public class GenerateQuestionWithAiCommandValidator : AbstractValidator<GenerateQuestionWithAiCommand>
{
    private const long MaxFileSizeInBytes = 15 * 1024 * 1024; // 50MB

    private const int MaxPageCount = 1000;


    public GenerateQuestionWithAiCommandValidator(IPdfService pdfService)
    {
        IPdfService pdfService1 = pdfService;

        RuleFor(x => x.FileData)
            .NotNull().WithMessage("FileData không được trống");

        RuleFor(x => x.FileData.Data)
            .NotNull().WithMessage("Dữ liệu stream không được trống")
            // .Must(stream => stream!.Length > 0).WithMessage("Stream không được rỗng")
            // .Must(stream => stream!.Length <= MaxFileSizeInBytes)
            .WithMessage("Dung lượng tệp không được vượt quá 15MB")
            .Custom((stream, context) =>
            {
                if (stream == null || stream.Length == 0)
                    throw new ErrorCodeException(ErrorCodes.FILE_EMPTY);

                if (stream.Length > MaxFileSizeInBytes)
                    throw new ErrorCodeException(ErrorCodes.FILE_TOO_LARGE);
                try
                {
                    pdfService1.TrValidatePdf(stream!, MaxPageCount);
                }
                catch (ArgumentException ex)
                {
                    context.AddFailure(ex.Message);
                }
            });;
        ;
    }
}

public class GenerateQuestionWithAiCommandHandler : IRequestHandler<GenerateQuestionWithAiCommand, List<CreateUpdateQuestionDto>>
{
    private readonly IAiGenerateService _aiGenerateService;
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public GenerateQuestionWithAiCommandHandler(IAiGenerateService aiGenerateService, IUser user, IApplicationDbContext context)
    {
        _aiGenerateService = aiGenerateService;
        _user = user;
        _context = context;
    }

    public async Task<List<CreateUpdateQuestionDto>> Handle(GenerateQuestionWithAiCommand request, CancellationToken cancellationToken)
    {
        var uploadResult = await _aiGenerateService.UploadFileAsync(request.FileData, cancellationToken);
        var systemInstruction = PromptProvider.GetGenerateQuestionInstructionSystemPrompt(request.IsGenerateExplain, request.Language!);
        var prompt = PromptProvider.GetGenerateQuestionPrompt(
            documentStructure: request.DocumentStructure,
            selectedParts: request.SelectedParts,
            selectedQuesiontype: request.QuestionTypes,
            questionCount: request.QuestionCount,
            isGenerateExplain: request.IsGenerateExplain
        );
        var currentUser = await _context.DomainUsers.FindAsync(_user.UserId!.Value, cancellationToken);


        if (currentUser!.IsPaymentLocked == true)
            throw new ErrorCodeException(ErrorCodes.PAYMENT_IN_PROGRESS);

        currentUser.IsPaymentLocked = true;
        await _context.SaveChangesAsync(cancellationToken);
        
        var systemSetting = await _context.SystemSettings
            .OrderByDescending(x => x.Created)
            .FirstOrDefaultAsync(cancellationToken);
        if (systemSetting == null)
            throw new ErrorCodeException(ErrorCodes.SYSTEM_SETTING_NOT_FOUND);
        
        try
        {
            //Đếm token của prompt
            var minimumExpectedTokens = await _aiGenerateService.CountTokenWithFileAsync(
                fileUri: uploadResult.FileUri,
                systemInstruction: systemInstruction,
                prompt: prompt,
                cancellationToken: cancellationToken);

            if (minimumExpectedTokens > systemSetting.MaxInputToken)
                throw new ErrorCodeException(ErrorCodes.AI_FILE_TOO_LARGE);

            var apiInputCost = (double)minimumExpectedTokens / 1_000_000 * systemSetting.InputCostPerMillionTokens;
            var apiMaxOutputCost = (double)systemSetting.MaxOutputToken / 1_000_000 * systemSetting.OutputCostPerMillionTokens;
            var minimumTotalPoint = (int)Math.Round(apiInputCost + apiMaxOutputCost + systemSetting.FixedSystemFee);


            if (currentUser!.Balance < minimumTotalPoint)
                throw new ErrorCodeException(ErrorCodes.INSUFFICIENT_BALANCE,
                    $"minimumTotalPoint: {minimumTotalPoint}, tokenCount: {minimumExpectedTokens}");


            var result = await _aiGenerateService.SendPromptWithFileAsync(
                fileUri: uploadResult.FileUri,
                systemInstruction: systemInstruction,
                prompt: prompt,
                temperature: 0.7,
                topP: 0.8,
                cancellationToken:  CancellationToken.None);

            currentUser.Balance -= (int)Math.Round(apiInputCost + systemSetting.FixedSystemFee);

            if(currentUser.Balance < 0)   currentUser.Balance  = 0;

            _context.DomainUsers.Update(currentUser);
            await _context.SaveChangesAsync(CancellationToken.None);

            if (result != "")
            {
                var outputToken = await _aiGenerateService.CountToken(
                    text1: "",
                    text2: result,
                    CancellationToken.None
                );

                var apiOutputCost = (double)outputToken / 1_000_000 * systemSetting.OutputCostPerMillionTokens;
                var totalPoint = (int)Math.Round(apiOutputCost);
                currentUser.Balance -= totalPoint;
                if(currentUser.Balance < 0)   currentUser.Balance  = 0;
                currentUser.IsPaymentLocked = false;
                _context.DomainUsers.Update(currentUser);
                await _context.SaveChangesAsync(CancellationToken.None);
            }


            //Xử lý chuỗi
            int startIndex = result.IndexOf('[');
            int endIndex = result.LastIndexOf(']');

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
            catch (ErrorCodeException)
            {
                throw;
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var document = JsonSerializer.Deserialize<List<CreateUpdateQuestionDto>>(normalizedResult, options);
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
                throw new ErrorCodeException(ErrorCodes.GENERATE_CONTENT_FAILED, $" {ex.Message}");
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
            await _context.SaveChangesAsync(CancellationToken.None);
            await _aiGenerateService.DeleteFileAsync(uploadResult.FileName);
        }
    }

}
