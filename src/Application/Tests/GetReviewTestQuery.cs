using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Application.Tests.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

[Authorize]
public class GetReviewTestQuery : IRequest<ReviewTestDto>
{
    public required Guid AttemptId { get; set; }
}

public class ReviewTestValidator : AbstractValidator<GetReviewTestQuery>
{
    public ReviewTestValidator()
    {
        RuleFor(x => x.AttemptId)
            .NotEmpty().WithMessage("AttemptId không được để trống");
    }
}

public class GetReviewTestQueryHandler : IRequestHandler<GetReviewTestQuery, ReviewTestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestService _testService;
    private readonly IUser _user;
    
    public GetReviewTestQueryHandler(
        IApplicationDbContext context,
        ITestService testService,
        IUser user)
    {
        _context = context;
        _testService = testService;
        _user = user;       
    }
    
    public async Task<ReviewTestDto> Handle(GetReviewTestQuery rq, CancellationToken cancellationToken)
    {
        var attempt = await _context.Attempts
            .Include(x => x.User)
            .Include(x => x.Test)
            .ThenInclude(x => x!.TestGrades)
            .Where(x => x.Id.Equals(rq.AttemptId)
            && x.TimeStart <= x.TimeFinish)
            .FirstOrDefaultAsync(cancellationToken);

        if (attempt == null || attempt.Test == null)
            throw new ErrorCodeException(ErrorCodes.ATTEMPT_NOT_FOUND);
        
        if(attempt.TimeStart > attempt.TimeFinish)
            throw new ErrorCodeException(ErrorCodes.NOT_SUBMITTED_CAN_NOT_VIEW);

        var roleInTest = await _testService.GetRoleUserInTest(attempt.Test);

        var isShowCorrectAnswer = attempt.Test.IsShowCorrectAnswerInReview;

        if (ClassShareMode.Owner == roleInTest || ClassShareMode.Teacher == roleInTest)
        {
            isShowCorrectAnswer = true;
        }
        else
        {
            if (!attempt.Test.IsAllowReviewAfterSubmit || !attempt.UserId.Equals(_user.UserId))
                throw new ErrorCodeException(ErrorCodes.STUDENT_CAN_REVIEW_THIS_TEST);
        }

        var totalPoint = attempt.Test.TestGrades.Where(x => x.UserId == attempt.UserId).FirstOrDefault()?.Score ?? 0;
        
        Dictionary<Guid, AttemptQuestion>? userAnswerDict = null;
        
        userAnswerDict = await _context.AttemptQuestions
            .Where(x => x.AttemptId == attempt.Id)
            .ToDictionaryAsync(a => a.QuestionId, a => a, cancellationToken);
        
        var versionQuestions = await _context.TestVersionQuestions
            .Include(x => x.Question)
            .Where(x => x.TestVersion!.Id == attempt.TestVersionId)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);
        
        var reviewTest = new ReviewTestDto
        {
            AttemptId = attempt.Id,
            Name = attempt.Test.Name,
            StudentName = attempt.User!.FullName,
            TimeStart = attempt.TimeStart,
            TimeEnd = attempt.TimeFinish,
            Score = totalPoint,
            Status = attempt.Test.PassingScore/100 <= totalPoint/versionQuestions.Sum(x => x.Question!.Score)
                ? nameof(AttemptStatus.Passed) : nameof(AttemptStatus.Failed),
        };
            
        reviewTest.Questions = versionQuestions
            .OrderBy(x => x.Order)
            .Select(q =>
            {
                AttemptQuestion? ans = null;
                if (userAnswerDict != null)
                    userAnswerDict.TryGetValue(q.Question!.Id, out ans);

                return ReviewQuestionDto.Mapper.FromEntity(q.Question!, ans!=null && !ans.DataJson.Equals("[]") ? ans : null, isShowCorrectAnswer);
            })
            .ToList();

        return reviewTest;
    }
}
