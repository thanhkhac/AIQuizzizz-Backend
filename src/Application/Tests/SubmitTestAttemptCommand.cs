using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Application.Tests.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

public class TestResultDto
{
    public DateTimeOffset TimeStart;
    public DateTimeOffset TimeEnd;
    public float Score;
}

[Authorize]
public class SubmitTestAttemptCommand : IRequest<TestResultDto>
{
    /// <summary>
    /// AttemptId of the test the student chose to take
    /// </summary>
    public Guid AttemptId { get; set; }
    public List<UserAnswerDto> UserAnswers { get; set; } = new();
    public bool IsSubmit { get; set; } = false;
}

public class SubmitTestAttemptCommandValidator : AbstractValidator<SubmitTestAttemptCommand>
{
    public SubmitTestAttemptCommandValidator()
    {
        RuleFor(x => x.AttemptId)
            .NotEmpty().WithMessage("TestId không được rỗng");
    }
}

public class SubmitTestAttemptCommandHandler : IRequestHandler<SubmitTestAttemptCommand, TestResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestService _testService;
    private readonly IUser _user;
    private readonly IHangFireService _hangFireService;
    
    public SubmitTestAttemptCommandHandler(
        IApplicationDbContext context,
        ITestService testService,
        IUser user,
        IHangFireService hangFireService)
    {
        _context = context;
        _testService = testService;
        _user = user;
        _hangFireService = hangFireService;       
    }

    /// <summary>
    /// The function checks the student's questions and returns the test score
    /// </summary>
    /// <param name="rq">Request contains AttemptId information and student responses</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<TestResultDto> Handle(SubmitTestAttemptCommand rq, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts
            .Include(x => x.Test)
            .Include(x => x.TestVersion)
            .Where(x => x.Id == rq.AttemptId && x.Test!.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (attempt == null || attempt.TestVersion == null || attempt.Test == null) 
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Bài test không tồn tại");
        
        if (attempt.UserId != _user.UserId)
            throw new ErrorCodeException(ErrorCodes.ERROR_ATTEMPT_USER, "Người làm bài không phải student đã attempt");
        
        await _testService.TryCheckCanAttemptTest(attempt.Test);

        if (attempt.TimeFinish >= attempt.TimeStart)
        {
            throw new ErrorCodeException(ErrorCodes.ATTEMPT_ALREADY_SUBMIT);
        }

        var questionsInTest = await _context.TestVersionQuestions
            .Include(x => x.Question)
            .Include(x => x.TestVersion)
            .Where(t => t.TestVersion!.Id == attempt.TestVersionId)
            .Select(x => QuestionResponseDto.Mapper.FromEntity(x.Question!,true, true, true))
            .ToListAsync(cancellationToken);
        
        var userAnswers  = rq.UserAnswers.ToDictionary(q => q.QuestionId, q => q); 
        
        var attemptQuestions = new List<AttemptQuestion>();
        
        float totalScore = 0;
        
        var attemptedQuestions = await _context.AttemptQuestions
                .Where(x => x.AttemptId.Equals(rq.AttemptId))
                .ToListAsync(cancellationToken);
        
        
        foreach (var question in questionsInTest)
        {
            var attemptQuestion = attemptedQuestions.FirstOrDefault(x => x.QuestionId == question.Id);
            
            var newAttemptQuestion = new AttemptQuestion
            {
                Id = Guid.NewGuid(),
                AttemptId = rq.AttemptId,
                QuestionId = question.Id,
                DataJson = "[]",
                Score = 0
            };
            
            if (!userAnswers.TryGetValue(question.Id, out var userAnswer))
            {
                if (attemptQuestion == null)
                {
                    attemptQuestions.Add(newAttemptQuestion);   
                }
                continue;
            }
            
            float scoreGraded = question.Type switch
            {
                nameof(QuestionType.MultipleChoice) => CheckUserAnswer.CheckMultipleChoiceAnswer(userAnswer, question),
                nameof(QuestionType.Matching) => CheckUserAnswer.CheckMatchingAnswer(userAnswer, question),
                nameof(QuestionType.Ordering) => CheckUserAnswer.CheckOrderingAnswer(userAnswer, question),
                nameof(QuestionType.ShortText) => CheckUserAnswer.CheckShortTextAnswer(userAnswer, question),
                _ => throw new ErrorCodeException(ErrorCodes.INVALID_QUESTION_TYPE, $"Loại câu hỏi {question.Type} không được hỗ trợ")
            };

            if (attemptQuestion != null)
            {
                attemptQuestion.DataJson = Serializer.Serialize(userAnswer.UserAnswerData);
                attemptQuestion.Score = scoreGraded;
            }
            else
            {
                newAttemptQuestion.DataJson = Serializer.Serialize(userAnswer.UserAnswerData);
                newAttemptQuestion.Score = scoreGraded;
                attemptQuestions.Add(newAttemptQuestion);
            }
            totalScore += scoreGraded;
        }

        if (rq.IsSubmit)
        {
            attempt.TimeFinish = DateTime.UtcNow;
            await _hangFireService.DeleteJobByArgument(attempt.Id.ToString());
        }

        var userGrade = await _context.TestGrades
                .Where(x => x.UserId == _user.UserId && x.TestId == attempt.TestId)
                .FirstOrDefaultAsync(cancellationToken);

        if (userGrade == null)
        {
            userGrade = new TestGrade
            {
                    Id = Guid.NewGuid(),
                    Score = totalScore,
                    TestId = attempt.TestId,
                    UserId = _user.UserId!.Value,
            };
            
                _context.TestGrades.Add(userGrade);
        }
        else
        {
            if (GradeAttemptMethod.HighestScore.Equals(attempt.Test.GradeAttemptMethod))
            {
                var allAttempt = await _context.Attempts
                    .Where(a => a.TestId == attempt.TestId && a.UserId == _user.UserId)
                    .OrderByDescending(a => a.Score)
                    .FirstOrDefaultAsync(cancellationToken);

                if (allAttempt!.Score < totalScore)
                {
                    userGrade.Score = totalScore;
                    _context.TestGrades.Update(userGrade);
                }
            }
            else
            {
                userGrade.Score = totalScore;
                _context.TestGrades.Update(userGrade);
            }
        }   
        
        _context.AttemptQuestions.AddRange(attemptQuestions);
        
        await _context.SaveChangesAsync(cancellationToken);

        return new TestResultDto
        {
            TimeStart = attempt.TimeStart,
            TimeEnd = attempt.TimeFinish,
            Score = totalScore,
        };
    }
}
