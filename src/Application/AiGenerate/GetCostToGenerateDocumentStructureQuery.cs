using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.AiGenerate.Services;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.AiGenerate;

public class GetCostToGenerateDocumentStructureQuery : IRequest<AiMinimumCostDto>
{
    public required FileStreamData FileData { get; set; }

}

public class CountDocumentTokenQueryValidator : AbstractValidator<GetCostToGenerateDocumentStructureQuery>
{
    private const long MaxFileSizeInBytes = 15 * 1024 * 1024; // 50MB

    private const int MaxPageCount = 1000;


    public CountDocumentTokenQueryValidator(IPdfService pdfService)
    {
        IPdfService pdfService1 = pdfService;
        
        RuleFor(x => x.FileData)
            .NotNull().WithMessage("FileData không được trống");

        RuleFor(x => x.FileData.Data)
            .NotNull().WithMessage("Dữ liệu stream không được trống")
            .Must(stream => stream!.Length > 0).WithMessage("Stream không được rỗng")
            .Must(stream => stream!.Length <= MaxFileSizeInBytes)
            .WithMessage("Dung lượng tệp không được vượt quá 50MB")
            .Custom((stream, context) =>
            {
                try
                {
                    pdfService1.TrValidatePdf(stream!, MaxPageCount);
                }
                catch (ArgumentException ex)
                {
                    context.AddFailure(ex.Message);
                }
            });;
    }
}

public class CountDocumentTokenQueryHandler : IRequestHandler<GetCostToGenerateDocumentStructureQuery, AiMinimumCostDto>
{
    private readonly IAiGenerateService _aiGenerateService;
    private readonly IApplicationDbContext _context;

    public CountDocumentTokenQueryHandler(IAiGenerateService aiGenerateService, IApplicationDbContext context)
    {
        _aiGenerateService = aiGenerateService;
        _context = context;
    }


    public async Task<AiMinimumCostDto> Handle(GetCostToGenerateDocumentStructureQuery request, CancellationToken cancellationToken)
    {
        var uploadResult = await _aiGenerateService.UploadFileAsync(request.FileData, cancellationToken);
        var systemInstruction = PromptProvider.GetGenerateDocumentStructureSystemInstructionPrompt();

        var systemSetting = await _context.SystemSettings
            .OrderByDescending(x => x.Created)
            .FirstOrDefaultAsync(cancellationToken);
        if (systemSetting == null)
            throw new ErrorCodeException(ErrorCodes.SYSTEM_SETTING_NOT_FOUND);
        
        try
        {
            var token = await _aiGenerateService.CountTokenWithFileAsync(
                fileUri: uploadResult.FileUri,
                systemInstruction: systemInstruction,
                prompt: "",
                cancellationToken: cancellationToken);

            if (token > systemSetting.MaxInputToken)
                throw new ErrorCodeException(ErrorCodes.AI_FILE_TOO_LARGE);

            var apiInputCost = (double)token / 1_000_000 * systemSetting.InputCostPerMillionTokens;
            var apiOutputCost = (double)systemSetting.MaxOutputToken / 1_000_000 * systemSetting.OutputCostPerMillionTokens;

            var minimumPoint = (int)Math.Round(apiInputCost + apiOutputCost + systemSetting.FixedSystemFee);

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
