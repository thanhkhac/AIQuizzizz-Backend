using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.AiGenerate;

public class GenerateQuestionWithAiCommand : IRequest<string>
{
    public required FileStreamData FileData { get; set; }
}

public class GenerateQuestionWithAiCommandValidator : AbstractValidator<GenerateQuestionWithAiCommand>
{
    private const long MaxFileSizeInBytes = 50 * 1024 * 1024; // 50MB

    public GenerateQuestionWithAiCommandValidator()
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

public class GenerateQuestionWithAiCommandHandler : IRequestHandler<GenerateQuestionWithAiCommand, string>
{
    private readonly IAiGenerateService _aiGenerateService;
    
    public GenerateQuestionWithAiCommandHandler(IAiGenerateService aiGenerateService)
    {
        _aiGenerateService = aiGenerateService;
    }
    
    public Task<string> Handle(GenerateQuestionWithAiCommand rq, CancellationToken cancellationToken)
    {
        // return _aiGenerateService.SendPromptWithFileAsync(rq.FileData);
        throw new NotImplementedException();
    }
    
}
