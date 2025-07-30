using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.QuestionSets.Commands;

public class GenerateQuestionWithAiCommand : IRequest<string>
{
    public required FileStreamData FileData { get; set; }
}

public class GenerateQuestionWithAiCommandValidator : AbstractValidator<GenerateQuestionWithAiCommand>
{
    public GenerateQuestionWithAiCommandValidator()
    {
        RuleFor(x => x.FileData)
            .NotNull().WithMessage("FileData không được trống");

        RuleFor(x => x.FileData.Data)
            .NotNull().WithMessage("Dữ liệu stream không được trống")
            .Must(stream => stream!.Length > 0).WithMessage("Stream không được rỗng");
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
        return _aiGenerateService.SendPromptWithFileAsync(rq.FileData);
    }
}
