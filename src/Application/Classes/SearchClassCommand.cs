using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Classes;

public class ClassSearchResultDto
{
    public required Guid ClassId { get; set; }
    public required string Name { get; set; }
    public Guid? OwnerId { get; set; }
    public string? Owner { get; set; }
    public string? Topic { get; set; }
}

[Authorize]
public class SearchClassCommand : IRequest<PaginatedList<ClassSearchResultDto>>
{
    public string? ShareMode { get; set; }
    public string? Name { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class SearchClassCommandValidator : AbstractValidator<SearchClassCommand>
{
        public SearchClassCommandValidator()
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

public class SearchClassCommandHandler : IRequestHandler<SearchClassCommand, PaginatedList<ClassSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public SearchClassCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>
    /// The function searches for classes based on name and share mode, returning a paginated list of class details
    /// </summary>
    /// <param name="rq">Request contains Name, ShareMode, PageNumber, and PageSize information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<PaginatedList<ClassSearchResultDto>> Handle(SearchClassCommand rq, CancellationToken cancellationToken)
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
            .Select(x => new
            {
                FullName = x.CreatedByUser != null ? x.CreatedByUser.FullName : null, x.Class.Id, OwnerId = x.CreatedByUser!.Id
            })
            .GroupBy(x => new {x.Id, x.FullName, x.OwnerId })
            .ToDictionaryAsync(
                x => x.Key.Id,
                x => new { FullName = x.Key.FullName, OwnerId = x.Key.OwnerId },
                cancellationToken
            );

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
                    Owner = classOwnerMap.ContainsKey(cl.Key.Id) ? classOwnerMap[cl.Key.Id].FullName : null,
                    Topic = cl.Key.Topic,
                    OwnerId = classOwnerMap.ContainsKey(cl.Key.Id) ? classOwnerMap[cl.Key.Id].OwnerId : null,
                })
                .AsQueryable(),
            rq.PageNumber,
            rq.PageSize
        );
    }
}
