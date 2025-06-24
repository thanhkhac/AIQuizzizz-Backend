using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Class;

public class ClassSearchResultDto
{
    public required string Name { get; set; }
    public string? Owner { get; set; }
}
public class SearchClass : IRequest<List<ClassSearchResultDto>>
{
    public ClassShareMode? ShareMode { get; set; }
    public string? Name { get; set; }
}

public class SearchClassHandler : IRequestHandler<SearchClass, List<ClassSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public SearchClassHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<ClassSearchResultDto>> Handle(SearchClass rq, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers.Where(x => x.Id == _user.UserId).FirstOrDefaultAsync();
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");

        var classes = _context.Classes
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
        
        return await classes
            .Where(c => c.ClassUser.UserId == user.Id)
            .GroupBy(x => new { x.Class.Id, x.Class.Name })
            .Select(cl => new ClassSearchResultDto
            {
                Name = cl.Key.Name,
                Owner = classOwnerMap.ContainsKey(cl.Key.Name) ? classOwnerMap[cl.Key.Name] : null
            })
            .ToListAsync(cancellationToken);
    }
}
