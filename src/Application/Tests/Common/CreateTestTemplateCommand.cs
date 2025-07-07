using System.Text.Json;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Common.Serializers;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Common;
using CleanArchitectureBase.Application.Tests.Common;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.Common;

[Authorize]
public class CreateTestTemplateCommand : IRequest<Guid>
{
    public required string Name { get; set; }
    public List<CreateQuestionDto> Questions { get; set; } = new ();
}

public class CreateTestTemplateCommandValidator : AbstractValidator<CreateTestTemplateCommand>
{
    public CreateTestTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên bộ câu hỏi không được để trống")
            .MaximumLength(200).WithMessage("Tên bộ câu hỏi không được vượt quá 200 ký tự");
        
        RuleFor(x => x.Questions)
            .Must(q => q != null && q.Count <= 500)
            .WithMessage("Bộ câu hỏi không được vượt quá 500 câu");
        
        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("Bộ câu hỏi phải chứa ít nhất một câu hỏi")
            .Must(questions => questions.All(IsValidQuestionType))
            .WithMessage("Một hoặc nhiều câu hỏi có loại hoặc dữ liệu không hợp lệ");
        
        RuleForEach(x => x.Questions)
            .SetValidator((command, question) => new QuestionCreateDtoValidator());
    }
    
    private bool IsValidQuestionType(CreateQuestionDto createQuestion)
    {
        bool isValid = createQuestion.Type switch
        {
            nameof(QuestionType.MultipleChoice) => createQuestion.MultipleChoices != null && createQuestion.MultipleChoices.Any(),
            nameof(QuestionType.Matching) => createQuestion.MatchingPairs != null && createQuestion.MatchingPairs.Any(),
            nameof(QuestionType.Ordering) => createQuestion.OrderingItems != null && createQuestion.OrderingItems.Any(),
            nameof(QuestionType.ShortText) => !string.IsNullOrWhiteSpace(createQuestion.ShortAnswer),
            _ => false
        };

        return isValid;
    }
}

public class CreateTestTemplateCommandHandler : IRequestHandler<CreateTestTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public readonly IUser _user;
    public readonly TestValidationService _testValidationService;

    public CreateTestTemplateCommandHandler(
        IApplicationDbContext context,
        IUser user,
        TestValidationService testValidationService)
    {
        _context = context;
        _user = user;
        _testValidationService = testValidationService;
    }
    
    public async Task<Guid> Handle(CreateTestTemplateCommand rq, CancellationToken cancellationToken)
    {
        await _testValidationService.ValidateQuestionAccessAsync(rq.Questions, cancellationToken);

        var testTemplate = new TestTemplate { Id = Guid.NewGuid(), Name = rq.Name, IsDeleted = false, };
        
        var listTemplateQuestions = new List<TestTemplateQuestion>();
        
        var listQuestions = new List<Question>();

        foreach (var questionDto in rq.Questions)
        {
            if (questionDto.QuestionId.HasValue)
            {
                var templateQuestion = new TestTemplateQuestion
                {
                    Id = Guid.NewGuid(), QuestionId = questionDto.QuestionId.Value, TestTemplateId = testTemplate.Id
                };
                
                listTemplateQuestions.Add(templateQuestion);   
            }
            else
            {
                var question = new Question
                {
                    Id = Guid.NewGuid(),
                    Type = Enum.Parse<QuestionType>(questionDto.Type!),
                    QuestionText = questionDto.QuestionText,
                    TextFormat = TextFormat.PlainText,
                    Score = questionDto.Score
                };

                var templateQuestion = new TestTemplateQuestion
                {
                    Id = Guid.NewGuid(), QuestionId = question.Id, TestTemplateId = testTemplate.Id, Question = question
                };
            
                question.DataJson = questionDto.Type switch
                {
                    nameof(QuestionType.MultipleChoice) when questionDto.MultipleChoices != null => QuestionTypeSerializer.SerializeMultipleChoice(questionDto.MultipleChoices),
                    nameof(QuestionType.Matching) when questionDto.MatchingPairs != null => QuestionTypeSerializer.SerializeMatchingPairs(questionDto.MatchingPairs),
                    nameof(QuestionType.Ordering) when questionDto.OrderingItems != null => QuestionTypeSerializer.SerializeOrderingItems(questionDto.OrderingItems),
                    nameof(QuestionType.ShortText) when !string.IsNullOrWhiteSpace(questionDto.ShortAnswer) => JsonSerializer.Serialize(
                        new QTypeShortAnswer { Answer = questionDto.ShortAnswer }),
                    _ => throw new InvalidDataException($"Dữ liệu câu hỏi không hợp lệ cho loại câu hỏi: {questionDto.Type}")
                };
            
                listQuestions.Add(question);
            
                listTemplateQuestions.Add(templateQuestion);   
            }
        }
        
        _context.TestTemplates.Add(testTemplate);
        
        _context.Questions.AddRange(listQuestions);
        
        _context.TestTemplateQuestions.AddRange(listTemplateQuestions);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return testTemplate.Id;
    }
}
