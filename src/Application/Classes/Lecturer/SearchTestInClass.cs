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
    public string? Status { get; set; }
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
        
        RuleFor(x => x.Status)
            .Must(mode => new[] {"Active", "Completed", "Upcoming"}.Contains(mode) || string.IsNullOrEmpty(mode))
            .WithMessage($"SharedMode phải là Active, Completed, Upcoming");
    }
}

public class SearchTestHandler : IRequestHandler<SearchTestInClass, PaginatedList<TestSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ClassValidationService _classValidationService;
    private readonly IUser _user;
    
    public SearchTestHandler(
        IApplicationDbContext context,
        ClassValidationService classValidationService,
        IUser user)
    {
        _context = context;
        _classValidationService = classValidationService;
        _user = user;
    }
    
    public async Task<PaginatedList<TestSearchResultDto>> Handle(SearchTestInClass rq, CancellationToken cancellationToken)
    {
        var user = await _context.ClassUsers
            .Include(cu => cu.User)
            .Where(cu => cu.UserId.Equals(_user.UserId) && cu.ClassId.Equals(rq.ClassId))
            .Select(cu => new { User = cu.User, ClassUser = cu })
            .FirstOrDefaultAsync(cancellationToken);
        
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_STUDENT_IN_CLASS, $"User with id {_user.UserId} not found in class");
        
        var tests = await _context.Tests.Where(x => x.ClassId.Equals(rq.ClassId))
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrEmpty(rq.TestName))
        {
            tests = tests.Where(x => x.Name.Contains(rq.TestName)).ToList();
        }

        switch (rq.Status)
        {
            case nameof(TestStatus.Active):
                tests = tests.Where(x => x.TimeStart <= DateTime.UtcNow && x.TimeFinish >= DateTime.UtcNow).ToList();
                break;
            case nameof(TestStatus.Completed):
                tests = tests.Where(x => x.TimeFinish < DateTime.UtcNow).ToList();
                break;
            case nameof(TestStatus.Upcoming):
                tests = tests.Where(x => x.TimeStart > DateTime.UtcNow).ToList();
                break;
        }
        
        var testIds = tests.Select(t => t.Id).ToList();
        
        var versionQuestionCounts = await _context.TestVersions
            .Include(tv => tv.TestVersionQuestions)
            .Where(tv => testIds.Contains(tv.TestId) && tv.No == 0)
            .Select(tv => new
            {
                TestId = tv.TestId,
                TestVersionId = tv.Id,
                QuestionCount = tv.TestVersionQuestions.Count()
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
