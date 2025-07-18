using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

public class SubmitTestAttemptCommand : IRequest<TestResultDto>
{
    /// <summary>
    /// AttemptId của bài test học sinh chọn làm
    /// </summary>
    public Guid AttemptId { get; set; }
    /// <summary>
    /// Các câu trả lời của học sinh
    /// </summary>
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
    
    public AttemptTestCommandHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }

    /// <summary>
    /// Hàm kiểm tra các câu hỏi của học sinh và trả về điểm bài làm
    /// </summary>
    /// <param name="rq">Request chứa thông tin AttemptId và các câu trả lời của học sinh</param>
    /// <param name="cancellationToken">Token để hủy tác vụ</param>
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
