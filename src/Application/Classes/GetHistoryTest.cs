using System.Linq.Expressions;
using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

public class HistoryTestDto
{
    public Guid AttemptId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public DateTime? TimeStart { get; set; }
    public DateTime? TimeSubmit { get; set; }
    public float Score { get; set; }
    public string? Status { get; set; }
}

public class GetHistoryTest : IRequest<PaginatedList<HistoryTestDto>>
{
    public required Guid TestId { get; set; }
    public string? StudentName { get; set; }
    public bool? IsPassed { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetHistoryTestValidator : AbstractValidator<GetHistoryTest>
{
    public GetHistoryTestValidator()
    {
        RuleFor(x => x.TestId)
            .NotEmpty().WithMessage("ClassId ko đc rỗng");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
            
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");
    }
}

public class GetHistoryTestHandler : IRequestHandler<GetHistoryTest, PaginatedList<HistoryTestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public GetHistoryTestHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<PaginatedList<HistoryTestDto>> Handle(GetHistoryTest rq, CancellationToken cancellationToken)
    {
        var test = await _context.Tests.Where(x => x.Id.Equals(rq.TestId)).FirstOrDefaultAsync(cancellationToken);
        if (test == null)
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Không tìm thấy bài test");
        
        await _classService.IsLecturerOrOwnerInClass(test.ClassId);

        var attempts = _context.Attempts
            .Include(x => x.User)
            .Include(x => x.Test)
            .Where(x => x.TestId == rq.TestId && 
                        (string.IsNullOrEmpty(rq.StudentName) || 
                         (x.User != null && x.User.FullName!.ToLower().Contains(rq.StudentName.ToLower()))))
            .OrderByDescending(x => x.TimeFinish)
            .Select(x => new HistoryTestDto
            {
                AttemptId = x.Id,
                StudentName = x.User != null ? x.User.FullName : null,
                StudentEmail = x.User != null ? x.User.Email : null,
                Score = x.Score,
                Status = x.Test!.PassingScore <= x.Score ? nameof(AttemptStatus.Passed) : nameof(AttemptStatus.Failed),
                TimeStart = x.TimeStart,
                TimeSubmit = x.TimeFinish,
            });
        
        if (rq.IsPassed != null)
            attempts = attempts
                .Where(x => x.Status!.Equals(rq.IsPassed.Value ? nameof(AttemptStatus.Passed) : nameof(AttemptStatus.Failed)));

        return await PaginatedList<HistoryTestDto>.CreateAsync(
            attempts.AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
