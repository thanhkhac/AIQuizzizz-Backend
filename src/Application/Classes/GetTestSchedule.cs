using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes;

public class TestScheduleResponse
{
    public DateTime Date { get; set; }
    public List<TestScheduleDto> TestSchedules { get; set; } = new();
}

public class TestScheduleDto
{
    public DateTime Date;
    public Guid TestId { get; set; }
    public string? TestName { get; set; }
}

[Authorize]
public class GetTestSchedule : IRequest<List<TestScheduleResponse>>
{
    public required Guid ClassId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
}

public class GetTestScheduleValidator : AbstractValidator<GetTestSchedule>
{
    public  GetTestScheduleValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được trống");

        RuleFor(x => x.Month)
            .NotEmpty().WithMessage("Month không được trống")
            .InclusiveBetween(1, 12).WithMessage("Tháng phải từ 1 đến 12");

        RuleFor(x => x.Year)
            .NotEmpty().WithMessage("Year không được trống")
            .GreaterThan(0).WithMessage("Năm phải lớn hơn 0");
    }
}

public class GetTestScheduleHandler : IRequestHandler<GetTestSchedule, List<TestScheduleResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public GetTestScheduleHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<List<TestScheduleResponse>> Handle(GetTestSchedule rq, CancellationToken cancellationToken)
    {  
        var classById = await _context.Classes
            .Where(x => x.Id == rq.ClassId)
            .FirstOrDefaultAsync(cancellationToken);
        if (classById == null)
            throw new ErrorCodeException(ErrorCodes.CLASS_NOTFOUND, "Không tìm thấy lớp");
        
        await _classService.IsUserInClass(rq.ClassId);

        var testSchedule = await _context.Tests
            .Where(t => t.ClassId.Equals(rq.ClassId) && t.TimeStart.Month == rq.Month && t.TimeStart.Year == rq.Year)
            .Select(x => new TestScheduleDto { TestId = x.Id, TestName = x.Name, Date = x.TimeStart.Date, })
            .ToListAsync(cancellationToken);
        
        var testScheduleResponse = testSchedule.GroupBy(x => x.Date)
            .Select(x => new TestScheduleResponse { Date = x.Key, TestSchedules = x.ToList() })
            .OrderBy(x => x.Date)
            .ToList();

        return testScheduleResponse;
    }
}
