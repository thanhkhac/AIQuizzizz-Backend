using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Application.Tests.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

public class CheckUpdateQuestion
{
    public List<Guid> NotUpdateQuestionIds { get; set; } = new();
    public List<Guid> UpdateQuestionIds { get; set; } = new();
}

[Authorize]
public class EditTestCommand : IRequest<Guid>
{
    public Guid TestId { get; set; }
    public required string Name { get; set; }
    public required int TimeLimit { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required string GradeAttemptMethod { get; set; }
    public required string GradeQuestionMethod { get; set; }
    public bool IsShowCorrectAnswerInReview { get; set; }
    public bool IsAllowReviewAfterSubmit { get; set; }
    public int MaxAttempt { get; set; } = 1;
    public int PassingScore { get; set; } = 0;
    public List<CreateUpdateQuestionDto> CreateUpdateQuestions { get; set; } = new ();
    public List<Guid> DeleteQuestionIds { get; set; } = new();
}

public class EditTestCommandValidator : AbstractValidator<EditTestCommand>
{
    public EditTestCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên bài kiểm tra không được để trống")
            .MaximumLength(200).WithMessage("Tên bài kiểm tra không được vượt quá 200 ký tự");

        RuleFor(x => x.TestId)
            .NotEmpty().WithMessage("TestId không được để trống");

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
        
        RuleForEach(x => x.CreateUpdateQuestions)
            .SetValidator((command, question) => new CreateUpdateQuestionDto.QuestionCreateDtoValidator());
    }
}

public class EditTestCommandHandler : IRequestHandler<EditTestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestService _testService;
    private readonly IClassService _classService;
    
    public EditTestCommandHandler(
        IApplicationDbContext context,
        ITestService testService,
        IClassService classService)
    {
        _context = context;
        _testService = testService;
        _classService = classService;
    }
    
    public async Task<Guid> Handle(EditTestCommand rq, CancellationToken cancellationToken)
    {
        var test = await _testService.CanEditTest(rq.TestId, cancellationToken);
        
        test.Name = rq.Name;
        test.TimeFinish = rq.EndTime;
        test.TimeStart = rq.StartTime;
        test.TimeLimit = rq.TimeLimit;
        test.MaxAttempt = rq.MaxAttempt;
        test.PassingScore = rq.PassingScore;
        test.IsShowCorrectAnswerInReview = rq.IsShowCorrectAnswerInReview;
        test.IsAllowReviewAfterSubmit = rq.IsAllowReviewAfterSubmit;
        test.GradeAttemptMethod = Enum.Parse<GradeAttemptMethod>(rq.GradeAttemptMethod);
        test.GradeQuestionMethod = Enum.Parse<GradeQuestionMethod>(rq.GradeQuestionMethod);

        var testVersion = await _context.TestVersions
            .Where(x => x.TestId.Equals(test.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        
        if (rq.CreateUpdateQuestions.Count > 100)
            throw new ErrorCodeException(ErrorCodes.NUMBER_OF_QUESTION_EXCEED_LIMIT,
                "Số lượng câu hỏi không được vượt quá 100");

        var versionQuestion = await _context.TestVersionQuestions
            .Include(x => x.TestVersion)
            .ThenInclude(x => x!.Test)
            .Include(x => x.Question)
            .Where(x => x.TestVersion!.Test!.Id == rq.TestId && x.TestVersion.No == 0)
            .Select(x => new {QuestionId = x.QuestionId, Question = x.Question})
            .ToListAsync(cancellationToken);
        
        var updateQuestionDto = rq.CreateUpdateQuestions
            .Where(x => x.QuestionId != null &&
                         versionQuestion.Select(y => y.QuestionId).Contains(x.QuestionId.Value))
            .ToList();

        var updateQuestion = versionQuestion
            .Where(x => updateQuestionDto.Any(q => q.QuestionId!.Value == x.QuestionId))
            .Select(x => x.Question!)
            .ToList();
        
        var newQuestionDto = rq.CreateUpdateQuestions
            .Where(x => x.QuestionId == null ||
                        !versionQuestion.Select(y => y.QuestionId).Contains(x.QuestionId.Value))
            .ToList();
        
        var newQuestionIds = await _testService.QuestionAccessAndCompareForTest(newQuestionDto, cancellationToken);

        var updateQuestionIds = await _testService.UpdateQuestion(updateQuestionDto, updateQuestion);
        
        var listQuestions = new List<Question>();
        
        var listTestVersionQuestions = new List<TestVersionQuestion>();

        var order = versionQuestion.Count;

        foreach (var questionDto in rq.CreateUpdateQuestions)
        {
            var questionId = Guid.NewGuid();

            if (questionDto.QuestionId.HasValue &&
                updateQuestionIds.NotUpdateQuestionIds.Contains(questionDto.QuestionId!.Value))
            {
                continue;
            }
            else if(questionDto.QuestionId.HasValue &&
                    updateQuestionIds.UpdateQuestionIds.Contains(questionDto.QuestionId!.Value))
            {
                var question = versionQuestion
                    .Where(x => x.QuestionId == questionDto.QuestionId)
                    .Select(x => x.Question!)
                    .FirstOrDefault();
                
                question!.QuestionText = questionDto.QuestionText;
                
                question.DataJson = CreateUpdateQuestionDto.Serializer.Serialize(questionDto);
                
                continue;
            }
            else if (questionDto.QuestionId.HasValue && newQuestionIds.Contains(questionDto.QuestionId!.Value))
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
                
                question.DataJson = CreateUpdateQuestionDto.Serializer.Serialize(questionDto);
                
                listQuestions.Add(question);

                questionId = question.Id;
            }

            foreach (var version in testVersion)
            {
                var testVersionQuestion = new TestVersionQuestion
                {
                    Id = Guid.NewGuid(), QuestionId = questionId, Order = order, TestVersionId = version
                };
                
                listTestVersionQuestions.Add(testVersionQuestion);
            }
            order++;
        }
        
        _context.Questions.AddRange(listQuestions);
        
        _context.TestVersionQuestions.AddRange(listTestVersionQuestions);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return test.Id;
    }
}
