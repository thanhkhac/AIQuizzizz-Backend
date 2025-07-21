using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

[Authorize]
public class SubmitTestAttemptCommand : IRequest<TestResultDto>
{
    /// <summary>
    /// AttemptId of the test the student chose to take
    /// </summary>
    public Guid AttemptId { get; set; }
    public List<UserAnswerDto> UserAnswers { get; set; } = new();
}

public class AttemptTestCommandValidator : AbstractValidator<SubmitTestAttemptCommand>
{
    public AttemptTestCommandValidator()
    {
        RuleFor(x => x.AttemptId)
            .NotEmpty().WithMessage("TestId không được rỗng");
    }
}

public class AttemptTestCommandHandler : IRequestHandler<SubmitTestAttemptCommand, TestResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    private readonly IUser _user;
    
    public AttemptTestCommandHandler(IApplicationDbContext context, IClassService classService, IUser user)
    {
        _context = context;
        _classService = classService;
        _user = user;
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
            .Where(x => x.Id == rq.AttemptId)
            .FirstOrDefaultAsync(cancellationToken);
        if (attempt == null || attempt.TestVersion == null || attempt.Test == null) 
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Bài test không tồn tại");
        
        attempt.TimeFinish = DateTime.UtcNow;
        
        await _classService.IsStudentInClass(attempt.Test.ClassId);

        var questionsInTest = await _context.TestVersionQuestions
            .Include(x => x.Question)
            .Include(x => x.TestVersion)
            .Where(t => t.TestVersion!.Id == attempt.TestVersionId)
            .Select(x => QuestionResponseDto.Mapper.FromEntity(x.Question!,true))
            .ToListAsync(cancellationToken);
        
        var userAnswers  = rq.UserAnswers.ToDictionary(q => q.QuestionId, q => q); 
        
        var attemptQuestions = new List<AttemptQuestion>();
        
        float totalScore = 0;
        
        foreach (var question in questionsInTest)
        {
            var attemptQuestion = new AttemptQuestion
            {
                Id = Guid.NewGuid(),
                AttemptId = rq.AttemptId,
                QuestionId = question.Id,
                DataJson = "[]",
                Score = 0
            };
            
            if (!userAnswers.TryGetValue(question.Id, out var userAnswer))
            {
                attemptQuestions.Add(attemptQuestion);
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

            attemptQuestion.DataJson = Serializer.Serialize(userAnswer.UserAnswerData);
            attemptQuestion.Score = scoreGraded;
            totalScore += scoreGraded;
            
            attemptQuestions.Add(attemptQuestion);
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
            Score = totalScore,
            TimeStart = attempt.TimeStart,
            TimeEnd = attempt.TimeFinish
        };
    }
}
