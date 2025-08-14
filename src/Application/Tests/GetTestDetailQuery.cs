using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Application.Tests.Service;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Tests;

[Authorize]
public class GetTestDetailQuery : IRequest<TestDetailDto>
{
    public required Guid TestId { get; set; }
    public bool? IsShowQuestion { get; set; } = true;
}

public class GetTestDetailQueryValidator : AbstractValidator<GetTestDetailQuery>
{
    public GetTestDetailQueryValidator()
    {
        RuleFor(x => x.TestId)
            .NotEmpty().WithMessage("TestId không được trống");
        
        
    }
}

public class GetTestDetailQueryHandler : IRequestHandler<GetTestDetailQuery, TestDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestService _testService;
    
    public GetTestDetailQueryHandler(IApplicationDbContext context, ITestService testService)
    {
        _context = context;
        _testService = testService;
    }
    
    public async Task<TestDetailDto> Handle(GetTestDetailQuery rq, CancellationToken cancellationToken)
    {
        var test = await _context.Tests
            .Include(x => x.TestVersions)
            .Where(x => x.Id == rq.TestId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (test == null)
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Không tìm thấy test");
        
        var canView = await _testService.CanViewOrEditTest(test.ClassId, cancellationToken);
        if (!canView)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST, "Không có quyền xem");
        
        var questions = rq?.IsShowQuestion ?? true ? _context.TestVersionQuestions
            .Include(x => x.TestVersion)
            .ThenInclude(x => x!.Test)
            .Include(x => x.Question)
            .Where(q => q.TestVersion!.Test!.Id == rq!.TestId && q.TestVersion.No == 0)
            .Select(qs => QuestionResponseDto.Mapper.FromEntity(qs.Question!, true, true, true))
            .ToList() : new List<QuestionResponseDto>();

        return new TestDetailDto
        {
            TestId = test.Id,
            ClassId = test.ClassId,
            Name = test.Name,
            StartTime = test.TimeStart,
            EndTime = test.TimeFinish,
            TimeLimit = test.TimeLimit,
            GradeAttemptMethod = test.GradeAttemptMethod.ToString(),
            GradeQuestionMethod = test.GradeQuestionMethod.ToString(),
            MaxAttempt = test.MaxAttempt,
            PassingScore = test.PassingScore,
            IsAllowReviewAfterSubmit = test.IsAllowReviewAfterSubmit,
            IsShowCorrectAnswerInReview = test.IsShowCorrectAnswerInReview,
            QuestionCount = test.QuestionCount,
            NumberOfShuffles = test.TestVersions.Count,
            Questions = questions
        };
    }
}
