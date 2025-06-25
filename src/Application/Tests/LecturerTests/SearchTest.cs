using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.LecturerTests;

public class TestSearchResultDto
{
    public required string Name { get; set; }
    public int? NumberOfQuestions { get; set; }
    public required DateTime TimeLimit { get; set; }
    public double RelativeTime { get; set; }
    public int? NumberOfCompletion { get; set; }
    public string? Status { get; set; }
}   

public class SearchTest : IRequest<List<TestSearchResultDto>>
{
    public required Guid ClassId { get; set; }
    public string? TestName { get; set; }
    public TestStatus? Status { get; set; }
}

public class SearchTestValidator : AbstractValidator<SearchTest>
{
    public SearchTestValidator()
    {
        RuleFor(v => v.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
    }
}

public class SearchTestHandler : IRequestHandler<SearchTest, List<TestSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    
    public SearchTestHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<TestSearchResultDto>> Handle(SearchTest rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes.FindAsync(rq.ClassId);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOT_FOUND, "Lớp học không tồn tại");
        
        var tests = await _context.Tests.Where(x => x.ClassId.Equals(rq.ClassId))
            .ToListAsync(cancellationToken);
        
        var testIds = tests.Select(t => t.Id).ToList();
        
        var testVersions = await _context.TestVersions
            .Where(x => testIds.Contains(x.TestId) && x.No == 0 )
            .Select(tv => new { tv.TestId, tv.Id })
            .ToDictionaryAsync(tv => tv.TestId, tv => tv.Id, cancellationToken);
        
        var testVersionIds = testVersions.Values.ToList();
        
        var questionCounts = await _context.TestVersionQuestions
            .Where(q => testVersionIds.Contains(q.TestVersionId))
            .GroupBy(q => q.TestVersionId)
            .Select(q => new { TestVersionId = q.Key, Count = q.Count() })
            .ToDictionaryAsync(x => x.TestVersionId, x => x.Count, cancellationToken);
        
        var attemptCounts = await _context.Attempts
            .Where(a => testIds.Contains(a.TestId))
            .GroupBy(a => a.TestId)
            .Select(g => new { TestId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TestId, x => x.Count, cancellationToken);

        var results = tests.Select(test =>
        {
            var testVersion = testVersions.ContainsKey(test.Id) ? testVersions[test.Id] : Guid.Empty;

            var questionCount = (testVersion != Guid.Empty && questionCounts.ContainsKey(testVersion))
                ? questionCounts[testVersion]
                : 0;

            var completionCount = attemptCounts.ContainsKey(test.Id) ? attemptCounts[test.Id] : 0;

            return new TestSearchResultDto
            {
                Name = test.Name,
                NumberOfQuestions = questionCount,
                NumberOfCompletion = completionCount,
                Status = test.TimeStart > DateTime.UtcNow ? TestStatus.Upcoming.ToString()
                    : test.TimeFinish < DateTime.UtcNow ? TestStatus.Completed.ToString()
                    : TestStatus.Active.ToString(),
                TimeLimit = test.TimeLimit,
                RelativeTime = Math.Floor((test.TimeStart - DateTime.UtcNow).TotalHours),
            };
        });
        
        return results.ToList();

    }

    
}
