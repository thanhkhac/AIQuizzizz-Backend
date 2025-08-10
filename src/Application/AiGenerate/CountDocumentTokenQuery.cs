using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.AiGenerate.Services;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.AiGenerate;

public class CountDocumentTokenQuery : IRequest<AiMinimumCostDto>
{
    public required FileStreamData FileData { get; set; }

}

public class CountDocumentTokenQueryValidator : AbstractValidator<GenerateDocumentStructureCommand>
{
    private const long MaxFileSizeInBytes = 50 * 1024 * 1024; // 50MB

    public CountDocumentTokenQueryValidator()
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

public class CountDocumentTokenQueryHandler : IRequestHandler<CountDocumentTokenQuery, AiMinimumCostDto>
{
    private readonly IAiGenerateService _aiGenerateService;

    public CountDocumentTokenQueryHandler(IAiGenerateService aiGenerateService)
    {
        _aiGenerateService = aiGenerateService;
    }


    public async Task<AiMinimumCostDto> Handle(CountDocumentTokenQuery request, CancellationToken cancellationToken)
    {
        var uploadResult = await _aiGenerateService.UploadFileAsync(request.FileData, cancellationToken);
        var systemInstruction = PromptProvider.GetGenerateDocumentStructureSystemInstructionPrompt();

        try
        {
            var token = await _aiGenerateService.CountTokenWithFileAsync(
                fileUri: uploadResult.FileUri,
                systemInstruction: systemInstruction,
                prompt: "",
                cancellationToken: cancellationToken);

            if (token > SystemSettings.MaxInputToken)
                throw new ErrorCodeException(ErrorCodes.AI_FILE_TOO_LARGE);

            var apiInputCost = (double)token / 1_000_000 * SystemSettings.InputCostPerMillionTokens;
            var apiOutputCost = (double)SystemSettings.MaxOutputToken / 1_000_000 * SystemSettings.OutputCostPerMillionTokens;

            var minimumPoint = (int)Math.Round(apiInputCost + apiOutputCost + SystemSettings.FixedSystemFee);

            return new AiMinimumCostDto
            {
                MiniumPointToGenerate = minimumPoint,
                TokenCount = token
            };
        }
        finally
        {
            await _aiGenerateService.DeleteFileAsync(uploadResult.FileName);
        }
    }
}
