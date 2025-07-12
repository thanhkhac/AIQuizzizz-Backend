using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class ClassSearchResultDto
{
    public required Guid ClassId { get; set; }
    public required string Name { get; set; }
    public string? Owner { get; set; }
}

[Authorize]
public class SearchClass : IRequest<PaginatedList<ClassSearchResultDto>>
{
    public ClassShareMode? ShareMode { get; set; }
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}

public class SearchClassValidator : AbstractValidator<SearchClass>
{
        public SearchClassValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
        }
}

public class SearchClassHandler : IRequestHandler<SearchClass, PaginatedList<ClassSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public SearchClassHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<PaginatedList<ClassSearchResultDto>> Handle(SearchClass rq, CancellationToken cancellationToken)
    {

        var classes = _context.Classes
            .Where(c => c.IsDeleted == false)
            .Include(c => c.ClassUsers)
            .ThenInclude(cu => cu.User)
            .SelectMany(c => c.ClassUsers, (c, cu) => new
            {
                Class = c,
                ClassUser = cu,
                User = cu.User
            });
        
        var classOwnerMap = await classes
            .Where(x => x.ClassUser.ShareMode == ClassShareMode.Owner)
            .Select(x => new { x.Class.Name, x.User.FullName })
            .ToDictionaryAsync(x => x.Name, x => x.FullName, cancellationToken);

        if (ClassShareMode.Student.Equals(rq.ShareMode) || ClassShareMode.Teacher.Equals(rq.ShareMode))
        {
            classes = classes
                .Where(x => x.ClassUser.ShareMode == ClassShareMode.Student
                            || x.ClassUser.ShareMode == ClassShareMode.Teacher);
        }
        
        if (ClassShareMode.Owner.Equals(rq.ShareMode))
        {
            classes = classes
                .Where(x => x.ClassUser.ShareMode.Equals(ClassShareMode.Owner));
        }
        
        if (!string.IsNullOrEmpty(rq.Name))
        {
            classes = classes
                .Where(x => x.Class.Name.Contains(rq.Name));
        }

        return await PaginatedList<ClassSearchResultDto>.CreateAsync(
            classes
                .Where(c => c.ClassUser.UserId == _user.UserId)
                .GroupBy(x => new { x.Class.Id, x.Class.Name })
                .Select(cl => new ClassSearchResultDto
                {
                    ClassId = cl.Key.Id,
                    Name = cl.Key.Name,
                    Owner = classOwnerMap.ContainsKey(cl.Key.Name) ? classOwnerMap[cl.Key.Name] : null
                }).AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
