using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Application.Tests.Service;

namespace CleanArchitectureBase.Application.Tests;

[Authorize]
public class GetTestDetailQuery : IRequest<TestDetailDto>
{
    public required Guid TestId { get; set; }
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
        var test = await _testService.CanViewTestDetails(rq.TestId, cancellationToken);
        
        var questions = _context.TestVersionQuestions
            .Include(x => x.TestVersion)
            .ThenInclude(x => x!.Test)
            .Include(x => x.Question)
            .Where(q => q.TestVersion!.Test!.Id == rq.TestId && q.TestVersion.No == 0)
            .Select(qs => QuestionResponseDto.Mapper.FromEntity(qs.Question!, true))
            .ToList();

        return new TestDetailDto
        {
            TestId = test.Id,
            Name = test.Name,
            TimeStart = test.TimeStart,
            TimeEnd = test.TimeFinish,
            TimeLimit = test.TimeLimit,
            GradeAttemptMethod = test.GradeAttemptMethod.ToString(),
            GradeQuestionValue = test.GradeQuestionMethod.ToString(),
            MaxAttempt = test.MaxAttempt,
            PassScore = test.PassingScore,
            IsAllowReviewAfterSubmit = test.IsAllowReviewAfterSubmit,
            IsShowCorrectAnswerInReview = test.IsShowCorrectAnswerInReview,
            QuestionCount = test.QuestionCount,
            Questions = questions
        };
    }
}
