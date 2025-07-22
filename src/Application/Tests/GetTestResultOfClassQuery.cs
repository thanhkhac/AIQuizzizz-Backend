using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests;

public class GetTestResultOfClassQuery : IRequest<PaginatedList<ResultTestOfClassDto>>
{
    public required Guid TestId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetTestResultOfClassQueryValidator : AbstractValidator<GetTestResultOfClassQuery>
{
    public GetTestResultOfClassQueryValidator()
    {
        RuleFor(x => x.TestId)
            .NotEmpty().WithMessage("ClassId ko đc rỗng");
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
            
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");
    }
}

public class GetTestResultOfClassQueryHandler : IRequestHandler<GetTestResultOfClassQuery, PaginatedList<ResultTestOfClassDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;
    
    public GetTestResultOfClassQueryHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<PaginatedList<ResultTestOfClassDto>> Handle(GetTestResultOfClassQuery rq, CancellationToken cancellationToken)
    {
        var test = await _context.Tests.Where(x => x.Id.Equals(rq.TestId)).FirstOrDefaultAsync(cancellationToken);
        if (test == null)
            throw new ErrorCodeException(ErrorCodes.TEST_NOT_FOUND, "Không tìm thấy bài test");

        var isLecturerOrOwnerInClass = await _classService.IsLecturerOrOwnerInClass(test.ClassId);
        if (!isLecturerOrOwnerInClass)
            throw new ErrorCodeException(ErrorCodes.NOT_FOUND_TEACHER_OR_OWNER_IN_CLASS, "Không phải lecturer hoặc owner của class");

        var resultTest = _context.TestGrades
            .Include(x => x.User)
            .Include(x => x.Test)
            .Where(x => x.TestId == rq.TestId)
            .Select(x => new ResultTestOfClassDto
            {
                StudentId = x.UserId,
                StudentName = x.User!.FullName,
                StudentEmail = x.User!.Email,
                Score = x.Score,
                Status = x.Score >= x.Test!.PassingScore
                    ? nameof(AttemptStatus.Passed)
                    : nameof(AttemptStatus.Failed)
            });
        
        return await PaginatedList<ResultTestOfClassDto>.CreateAsync(
            resultTest.AsQueryable(),
            rq.PageNumber,
            rq.PageSize
            ) ;
    }
}
