using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

[Authorize]
public class GetTestScheduleQuery : IRequest<List<TestScheduleResponse>>
{
    /// <summary>
    /// Id of the class want to retrieve the test schedule
    /// </summary>
    public int? Month { get; set; }
    public int? Year { get; set; }
}

public class GetTestScheduleQueryValidator : AbstractValidator<GetTestScheduleQuery>
{
    public  GetTestScheduleQueryValidator()
    {
        RuleFor(x => x.Month)
            .NotEmpty().WithMessage("Month không được trống")
            .InclusiveBetween(1, 12).WithMessage("Tháng phải từ 1 đến 12");

        RuleFor(x => x.Year)
            .NotEmpty().WithMessage("Year không được trống")
            .GreaterThan(0).WithMessage("Năm phải lớn hơn 0");
    }
}

public class GetTestScheduleQueryHandler : IRequestHandler<GetTestScheduleQuery, List<TestScheduleResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public GetTestScheduleQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;   
    }
    
    /// <summary>
    /// The function retrieves the test schedule for a class in a specified month and year, grouped by date
    /// </summary>
    /// <param name="rq">Request contains ClassId, Month, and Year information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<List<TestScheduleResponse>> Handle(GetTestScheduleQuery rq, CancellationToken cancellationToken)
    {  
        var testSchedule = await _context.Tests
            .Include(x => x.Class)
            .ThenInclude(x => x!.ClassUsers)
            .Where(t => t.TimeStart.Month == rq.Month && t.TimeStart.Year == rq.Year
            && t.Class!.ClassUsers.Any(x => x.UserId == _user.UserId && x.ShareMode == ClassShareMode.Student))
            .Select(x => new TestScheduleDto
            {
                TestId = x.Id,
                ClassId = x.ClassId,
                TestName = x.Name,
                Date = x.TimeStart.Date,
                ClassName = x.Class!.Name,
                TimeStart = x.TimeStart,
                Status = x.TimeStart > DateTime.UtcNow ? TestStatus.Upcoming.ToString()
                    : x.TimeFinish < DateTime.UtcNow ? TestStatus.Completed.ToString()
                    : TestStatus.Active.ToString()
            })
            .ToListAsync(cancellationToken);
        
        var testScheduleResponse = testSchedule
            .GroupBy(x => x.Date)
            .Select(x =>
                new TestScheduleResponse
                {
                    Date = x.Key,
                    TestSchedules = x.ToList()
                })
            .OrderBy(x => x.Date)
            .ToList();

        return testScheduleResponse;
    }
}
