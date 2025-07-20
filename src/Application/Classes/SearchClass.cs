using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

public class ClassSearchResultDto
{
    public required Guid ClassId { get; set; }
    public required string Name { get; set; }
    public string? Owner { get; set; }
    public string? Topic { get; set; }
}

[Authorize]
public class SearchClass : IRequest<PaginatedList<ClassSearchResultDto>>
{
    public string? ShareMode { get; set; }
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SearchClassValidator : AbstractValidator<SearchClass>
{
        public SearchClassValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
            
            RuleFor(x => x.ShareMode)
                .Must(mode => new[] {"Student", "Teacher", "Owner"}.Contains(mode) || string.IsNullOrEmpty(mode))
                .WithMessage($"SharedMode phải là Student, Teacher, Owner");
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
            .Include(x => x.CreatedByUser)
            .Include(c => c.ClassUsers)
            .SelectMany(c => c.ClassUsers, (c, cu) => new
            {
                Class = c,
                ClassUser = cu,
                CreatedByUser = c.CreatedByUser
            })
            .Where(x => (string.IsNullOrEmpty(rq.Name) || x.Class.Name.ToLower().Contains(rq.Name.ToLower())) &&
                        x.ClassUser.UserId.Equals(_user.UserId))
            .OrderByDescending(x => x.Class.LastModified)
            .AsQueryable();
        
        var classOwnerMap = await classes
            .Select(x => new {FullName = x.CreatedByUser != null ? x.CreatedByUser.FullName : null, x.Class.Id })
            .GroupBy(x => new {x.Id, x.FullName })
            .ToDictionaryAsync(x => x.Key.Id, x => x.Key.FullName, cancellationToken);

        if (!string.IsNullOrEmpty(rq.ShareMode) && Enum.TryParse<ClassShareMode>(rq.ShareMode, out var shareMode))
        {
            classes = classes.Where(x => x.ClassUser.ShareMode == shareMode);
        }

        return await PaginatedList<ClassSearchResultDto>.CreateAsync(
            classes
                .Where(c => c.ClassUser.UserId == _user.UserId)
                .GroupBy(x => new { x.Class.Id, x.Class.Name, x.Class.LastModified, x.Class.Topic})
                .OrderByDescending(x => x.Key.LastModified)
                .Select(cl => new ClassSearchResultDto
                {
                    ClassId = cl.Key.Id,
                    Name = cl.Key.Name,
                    Owner = classOwnerMap.ContainsKey(cl.Key.Id) ? classOwnerMap[cl.Key.Id] : null,
                    Topic = cl.Key.Topic,
                })
                .AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
