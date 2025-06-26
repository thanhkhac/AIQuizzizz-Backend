using System.Reflection;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;
using System.Linq.Dynamic.Core;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Classes.LecturerClasses;

public class StudentSearchResultDto
{
    public string? FullName { get; set; }
    public required string Email { get; set; }
    public ClassShareMode Position { get; set; }
}
public class SearchStudent : IRequest<PaginatedList<StudentSearchResultDto>>
{
    public required Guid ClassId { get; set; }
    public string? Keyword { get; set; }
    public string? FieldName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchStudentValidator : AbstractValidator<SearchStudent>
{
    public SearchStudentValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
    }
}

public class SearchStudentCommandHandler : IRequestHandler<SearchStudent, PaginatedList<StudentSearchResultDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<PaginatedList<StudentSearchResultDto>> Handle(SearchStudent rq, CancellationToken cancellationToken)
    {
        var classById = await _context.Classes.FindAsync(rq.ClassId);
        if (classById == null)  
            throw new ErrorCodeException(ErrorCodes.CLASS_NOT_FOUND, "Lớp học không tồn tại");
        
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
                Email = st.User.Email,
                FullName = st.User.FullName,
                Position = st.ShareMode
            }).AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
