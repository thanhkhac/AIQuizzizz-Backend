using System.Text.Json;
using CleanArchitectureBase.Application.Classes.Common;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Serializers;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Common;
using CleanArchitectureBase.Application.Tests.Common;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class CreateTestCommand : IRequest<Guid>
{
    public required string Name { get; set; }
    public required Guid ClassId { get; set; }
    public required int TimeLimit { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required string GradeAttemptMethod { get; set; }
    public required string GradeQuestionMethod { get; set; }
    public bool IsShowCorrectAnswerInReview { get; set; }
    public List<CreateQuestionDto> Questions { get; set; } = new ();
}

public class CreateTestCommandValidator : AbstractValidator<CreateTestCommand>
{
    public CreateTestCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên bài kiểm tra không được để trống")
            .MaximumLength(200).WithMessage("Tên bài kiểm tra không được vượt quá 200 ký tự");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");

        RuleFor(x => x.TimeLimit)
            .GreaterThan(0).WithMessage("Thời gian làm bài phải lớn hơn 0")
            .LessThanOrEqualTo(180).WithMessage("Thời gian làm bài không được vượt quá 180 phút");  

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Thời gian bắt đầu không được để trống")
            .Must(startTime => startTime > DateTime.UtcNow)
            .WithMessage("Thời gian bắt đầu phải lớn hơn thời gian hiện tại");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("Thời gian kết thúc không được để trống")
            .Must((command, endTime) => endTime > command.StartTime)
            .WithMessage("Thời gian kết thúc phải lớn hơn thời gian bắt đầu");
        
        RuleFor(x => x.GradeAttemptMethod)
            .NotEmpty().WithMessage($"GradeAttemptMethod không được để trống")
            .Must(type => new[] {"LastAttempt", "HighestScore"}.Contains(type))
            .WithMessage($"Loại câu hỏi phải là LastAttempt, HighestScore");
        
        RuleFor(x => x.GradeQuestionMethod)
            .NotEmpty().WithMessage($"GradeAttemptMethod không được để trống")
            .Must(type => new[] {"Partial", "AllOrNothing"}.Contains(type))
            .WithMessage($"Loại câu hỏi phải là Partial, AllOrNothing");
        
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

public class CreateTestCommandHandler : IRequestHandler<CreateTestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    public readonly IUser _user;
    public readonly TestValidationService _testValidationService;
    private readonly ClassValidationService _classValidationService;

    
    public CreateTestCommandHandler(
        IApplicationDbContext context,
        IUser user,
        TestValidationService testValidationService,
        ClassValidationService classValidationService)
    {
        _context = context;
        _user = user;
        _testValidationService = testValidationService;
        _classValidationService = classValidationService;
    }
    
    public async Task<Guid> Handle(CreateTestCommand rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classValidationService.ValidateClassAccessAsync(rq.ClassId, cancellationToken);
        
        var test = new Test
        {
            Id = Guid.NewGuid(),
            Name = rq.Name,
            ClassId = rq.ClassId,
            GradeAttemptMethod = Enum.Parse<GradeAttemptMethod>(rq.GradeAttemptMethod),
            GradeQuestionMethod = Enum.Parse<GradeQuestionMethod>(rq.GradeQuestionMethod),
            TimeFinish = rq.EndTime,
            TimeStart = rq.StartTime,
            TimeLimit = rq.TimeLimit,
            QuestionCount = 0
        };

        var testVersion = new TestVersion { Id = Guid.NewGuid(), TestId = test.Id, No = 0, };

        if (rq.Questions.Count > 100)
            throw new ErrorCodeException(ErrorCodes.NUMBER_OF_QUESTION_EXCEED_LIMIT, "Số lượng câu hỏi không được vượt quá 100");
        
        await _testValidationService.ValidateQuestionAccessAsync(rq.Questions, cancellationToken);
        
        var listQuestions = new List<Question>();
        
        var listTestVersionQuestions = new List<TestVersionQuestion>();

        int order  = 0;

        foreach (var questionDto in rq.Questions)
        {
            var questionId = Guid.NewGuid();
            
            if (questionDto.QuestionId.HasValue)
            {
                questionId = questionDto.QuestionId.Value;
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
                
                questionId = question.Id;
            }

            var testVersionQuestion = new TestVersionQuestion
            {
                Id = Guid.NewGuid(), QuestionId = questionId, Order = order, TestVersionId = testVersion.Id
            };
            
            order++;
            
            listTestVersionQuestions.Add(testVersionQuestion);
        }
        
        _context.Tests.Add(test);
        
        _context.TestVersions.Add(testVersion);
        
        _context.Questions.AddRange(listQuestions);
        
        _context.TestVersionQuestions.AddRange(listTestVersionQuestions);
        
        await _context.SaveChangesAsync(cancellationToken);

        return test.Id;
    }
}
