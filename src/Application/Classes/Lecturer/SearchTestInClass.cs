using CleanArchitectureBase.Application.Classes.Common;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class TestSearchResultDto
{
    public required Guid TestId { get; set; }
    public required string Name { get; set; }
    public int? NumberOfQuestions { get; set; }
    public required int TimeLimit { get; set; }
    public double RelativeTime { get; set; }
    public int? NumberOfCompletion { get; set; }
    public string? Status { get; set; }
}   

[Authorize]
public class SearchTestInClass : IRequest<PaginatedList<TestSearchResultDto>>
{
    public required Guid ClassId { get; set; }
    public string? TestName { get; set; }
    public TestStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchTestValidator : AbstractValidator<SearchTestInClass>
{
    public SearchTestValidator()
    {
        RuleFor(v => v.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
    }
}

public class SearchTestHandler : IRequestHandler<SearchTestInClass, PaginatedList<TestSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ClassValidationService _classValidationService;
    
    public SearchTestHandler(IApplicationDbContext context, ClassValidationService classValidationService)
    {
        _context = context;
        _classValidationService = classValidationService;
    }
    
    public async Task<PaginatedList<TestSearchResultDto>> Handle(SearchTestInClass rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classValidationService.ValidateClassAccessAsync(rq.ClassId, cancellationToken);
        
        var tests = await _context.Tests.Where(x => x.ClassId.Equals(rq.ClassId))
            .ToListAsync(cancellationToken);
        
        var testIds = tests.Select(t => t.Id).ToList();
        
        var versionQuestionCounts = await _context.TestVersions
            .Where(x => testIds.Contains(x.TestId) && x.No == 0)
            .GroupJoin(_context.TestVersionQuestions,
                tv => tv.Id,
                q => q.TestVersionId,
                (tv, qs) => new
                {
                    TestId = tv.TestId,
                    TestVersionId = tv.Id,
                    QuestionCount = qs.Count()
                })
            .ToDictionaryAsync(
                x => x.TestId,
                x => new { x.TestVersionId, x.QuestionCount },
                cancellationToken);
        
        var attemptCounts = await _context.Attempts
            .Where(a => testIds.Contains(a.TestId))
            .GroupBy(a => a.TestId)
            .Select(g => new { TestId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TestId, x => x.Count, cancellationToken);

        var results = tests.Select(test =>
        {
            var questionCount = versionQuestionCounts.ContainsKey(test.Id)
                ? versionQuestionCounts[test.Id]
                : new { TestVersionId = Guid.Empty, QuestionCount = 0 };

            var completionCount = attemptCounts.ContainsKey(test.Id) ? attemptCounts[test.Id] : 0;

            return new TestSearchResultDto
            {
                TestId = test.Id,
                Name = test.Name,
                NumberOfQuestions = questionCount.QuestionCount,
                NumberOfCompletion = completionCount,
                Status = test.TimeStart > DateTime.UtcNow ? TestStatus.Upcoming.ToString()
                    : test.TimeFinish < DateTime.UtcNow ? TestStatus.Completed.ToString()
                    : TestStatus.Active.ToString(),
                TimeLimit = test.TimeLimit,
                RelativeTime = Math.Floor((DateTime.UtcNow - test.TimeStart).TotalHours),
            };
        });

        return PaginatedList<TestSearchResultDto>.Create(
            results.ToList(),
            rq.PageNumber,
            rq.PageSize
        );
    }

    
}
