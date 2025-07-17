using System.Text.Json;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Utils;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.Tests.Service;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

[Authorize]
public class CreateTestTemplateCommand : IRequest<Guid>
{
    public required string Name { get; set; }
    public List<CreateUpdateQuestionDto> Questions { get; set; } = new ();
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
            .SetValidator((command, question) => new CreateUpdateQuestionDto.QuestionCreateDtoValidator());
    }
    
    private bool IsValidQuestionType(CreateUpdateQuestionDto createUpdateQuestion)
    {
        bool isValid = createUpdateQuestion.Type switch
        {
            nameof(QuestionType.MultipleChoice) => createUpdateQuestion.MultipleChoices != null && createUpdateQuestion.MultipleChoices.Any(),
            nameof(QuestionType.Matching) => createUpdateQuestion.MatchingPairs != null && createUpdateQuestion.MatchingPairs.Any(),
            nameof(QuestionType.Ordering) => createUpdateQuestion.OrderingItems != null && createUpdateQuestion.OrderingItems.Any(),
            nameof(QuestionType.ShortText) => !string.IsNullOrWhiteSpace(createUpdateQuestion.ShortAnswer),
            _ => false
        };

        return isValid;
    }
}

public class CreateTestTemplateCommandHandler : IRequestHandler<CreateTestTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public readonly IUser _user;
    public readonly ITestService _testService;

    public CreateTestTemplateCommandHandler(
        IApplicationDbContext context,
        IUser user,
        ITestService testService)
    {
        _context = context;
        _user = user;
        _testService = testService;
    }
    
    public async Task<Guid> Handle(CreateTestTemplateCommand rq, CancellationToken cancellationToken)
    {
        await _testService.QuestionAccess(rq.Questions, cancellationToken);

        var testTemplate = new TestTemplate { Id = Guid.NewGuid(), Name = rq.Name, IsDeleted = false, };

        var testTemplateUser = new TestTemplateUser
        {
            UserId = _user.UserId!.Value,
            TestTemplateId = testTemplate.Id,
            ShareMode = TestTemplateUserShareMode.Owner
        };
        
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
            
                question.DataJson = CreateUpdateQuestionDto.Serializer.Serialize(questionDto);
            
                listQuestions.Add(question);
            
                listTemplateQuestions.Add(templateQuestion);   
            }
        }
        
        _context.TestTemplates.Add(testTemplate);
        
        _context.TestTemplateUsers.Add(testTemplateUser);
        
        _context.Questions.AddRange(listQuestions);
        
        _context.TestTemplateQuestions.AddRange(listTemplateQuestions);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return testTemplate.Id;
    }
}
