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
        // Lọc rộng thêm ±1 ngày (UTC) để không bỏ sót bài test nằm sát biên tháng khi client ở múi giờ khác UTC.
        // Frontend sẽ gom nhóm/lọc lại theo ngày địa phương dựa trên TimeStart/TimeFinish thật.
        var monthStart = new DateTimeOffset(rq.Year!.Value, rq.Month!.Value, 1, 0, 0, 0, TimeSpan.Zero);
        var rangeFrom = monthStart.AddDays(-1);
        var rangeTo = monthStart.AddMonths(1).AddDays(1);
        var userId = _user.UserId;

        // Học viên, giáo viên và chủ lớp đều thấy lịch test của lớp mình tham gia
        var testSchedule = await _context.Tests
            .Where(t => !t.IsDeleted
                && t.TimeStart >= rangeFrom && t.TimeStart < rangeTo
                && t.Class != null && !t.Class.IsDeleted
                && t.Class.ClassUsers.Any(x => x.UserId == userId
                    && (x.ShareMode == ClassShareMode.Student
                        || x.ShareMode == ClassShareMode.Teacher
                        || x.ShareMode == ClassShareMode.Owner)))
            .Select(x => new TestScheduleDto
            {
                TestId = x.Id,
                ClassId = x.ClassId,
                TestName = x.Name,
                Date = x.TimeStart,
                ClassName = x.Class!.Name,
                TimeStart = x.TimeStart,
                TimeFinish = x.TimeFinish,
                Status = x.TimeStart > DateTime.UtcNow ? TestStatus.Upcoming.ToString()
                    : x.TimeFinish < DateTime.UtcNow ? TestStatus.Completed.ToString()
                    : TestStatus.Active.ToString()
            })
            .ToListAsync(cancellationToken);

        var testScheduleResponse = testSchedule
            .GroupBy(x => x.TimeStart.UtcDateTime.Date)
            .Select(x =>
                new TestScheduleResponse
                {
                    Date = new DateTimeOffset(x.Key, TimeSpan.Zero),
                    TestSchedules = x.ToList()
                })
            .OrderBy(x => x.Date)
            .ToList();

        return testScheduleResponse;
    }
}
