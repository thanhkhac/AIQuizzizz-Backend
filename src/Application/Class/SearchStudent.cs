using System.Reflection;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;
using System.Linq.Dynamic.Core;

namespace CleanArchitectureBase.Application.Class;

public class StudentDetailDto
{
    public string? FullName { get; set; }
    public required string Email { get; set; }
    public ClassShareMode Position { get; set; }
}
public class SearchStudent : IRequest<List<StudentDetailDto>>
{
    public required Guid ClassId { get; set; }
    public string? Keyword { get; set; }
    public string? FieldName { get; set; }
}

public class SearchStudentValidator : AbstractValidator<SearchStudent>
{
    public SearchStudentValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ClassId không được để trống");
    }
}

public class SearchStudentCommandHandler : IRequestHandler<SearchStudent, List<StudentDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<StudentDetailDto>> Handle(SearchStudent rq, CancellationToken cancellationToken)
    {
        var listStudent = _context.ClassUsers
            .Include(x => x.User)
            .Where(s => s.ClassId.Equals(rq.ClassId));
        
        if (!string.IsNullOrEmpty(rq.Keyword) && !string.IsNullOrEmpty(rq.FieldName))
        {
            var property = typeof(User).GetProperty(rq.FieldName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || property.PropertyType != typeof(string))
                throw new ArgumentException("Trường tìm kiếm không hợp lệ hoặc không phải kiểu string");

            string query = $"User.{rq.FieldName}.ToLower().Contains(@0)"; 
            listStudent = listStudent.Where(query, rq.Keyword.ToLower()); 
        }
        
        return await listStudent.Select(st => new StudentDetailDto
        {
            Email = st.User.Email,
            FullName = st.User.FullName,
            Position = st.ShareMode
        }).ToListAsync(cancellationToken);
    }
}
