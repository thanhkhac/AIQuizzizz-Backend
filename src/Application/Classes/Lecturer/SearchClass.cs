using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes.Lecturer;

public class ClassSearchResultDto
{
    public required string Name { get; set; }
    public string? Owner { get; set; }
}
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
        var user = await _context.DomainUsers
            .Where(x => x.Id == _user.UserId && x.IsDeleted == false && x.IsBanned == false)
            .FirstOrDefaultAsync();
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with id {_user.UserId} not found");

        var classes = _context.Classes
            .Where(c => c.IsDeleted == false)
            .Join(_context.ClassUsers,
                c => c.Id,
                cu => cu.ClassId,
                (c, cu) => new { Class = c, ClassUser = cu })
            .Join(_context.DomainUsers,
                cu => cu.ClassUser.UserId,
                u => u.Id,
                (cu, u) => new { Class = cu.Class, ClassUser = cu.ClassUser, User = u });
        
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
                .Where(c => c.ClassUser.UserId == user.Id)
                .GroupBy(x => new { x.Class.Id, x.Class.Name })
                .Select(cl => new ClassSearchResultDto
                {
                    Name = cl.Key.Name,
                    Owner = classOwnerMap.ContainsKey(cl.Key.Name) ? classOwnerMap[cl.Key.Name] : null
                }).AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
