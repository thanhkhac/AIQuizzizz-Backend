using System.Reflection;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;
using System.Linq.Dynamic.Core;
using CleanArchitectureBase.Application.Classes.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class StudentSearchResultDto
{
    public required Guid StudentId { get; set; }
    public string? FullName { get; set; }
    public required string Email { get; set; }
    public string? Position { get; set; }
}

[Authorize]
public class SearchStudentInClass : IRequest<PaginatedList<StudentSearchResultDto>>
{
    public required Guid ClassId { get; set; }
    public string? Keyword { get; set; }
    public string? FieldName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchStudentInClassValidator : AbstractValidator<SearchStudentInClass>
{
    public SearchStudentInClassValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
    }
}

public class SearchStudentCommandHandler : IRequestHandler<SearchStudentInClass, PaginatedList<StudentSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IClassService _classService;


    public SearchStudentCommandHandler(IApplicationDbContext context, IClassService classService)
    {
        _context = context;
        _classService = classService;
    }
    
    public async Task<PaginatedList<StudentSearchResultDto>> Handle(SearchStudentInClass rq, CancellationToken cancellationToken)
    {
        var (isOwner, classExists) = await _classService.GetClassOwnerAccess(rq.ClassId, cancellationToken);
        
        var listStudent = _context.ClassUsers
            .Include(x => x.User)
            .Where(s => s.ClassId.Equals(rq.ClassId) && s.ShareMode != ClassShareMode.Owner);
        
        if (!string.IsNullOrEmpty(rq.Keyword) && !string.IsNullOrEmpty(rq.FieldName))
        {
            var property = typeof(User).GetProperty(rq.FieldName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || property.PropertyType != typeof(string))
                throw new ErrorCodeException(ErrorCodes.FIELD_NAME_NOT_FOUND, "Trường tìm kiếm không hợp lệ hoặc không phải kiểu string");

            string query = $"User.{rq.FieldName}.ToLower().Contains(@0)"; 
            listStudent = listStudent.Where(query, rq.Keyword.ToLower()); 
        }
        
        return await PaginatedList<StudentSearchResultDto>.CreateAsync(
            listStudent.Select(st => new StudentSearchResultDto
            {
                StudentId = st.UserId,
                Email = st.User.Email,
                FullName = st.User.FullName,
                Position = st.ShareMode.ToString()
            }).AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
