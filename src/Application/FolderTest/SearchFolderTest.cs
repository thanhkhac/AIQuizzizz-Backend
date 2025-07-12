using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest;

public class SearchFolderTestDto
{
    public Guid FolderTestId { get; set; }
    public string? Name { get; set; }
    public string? SharedBy { get; set; }
}

[Authorize]
public class SearchFolderTest : IRequest<PaginatedList<SearchFolderTestDto>>
{
    public string? FolderName { get; set; }
    public string? SharedMode { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class SearchFolderTestValidator : AbstractValidator<SearchFolderTest>
{
    public SearchFolderTestValidator()
    {
        RuleFor(x => x.SharedMode)
            .Must(mode => new[] {"Owner", "Editable", "ViewOnly"}.Contains(mode) || string.IsNullOrEmpty(mode))
            .WithMessage($"SharedMode phải là Owner, Editable, ViewOnly");
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100");
    }
}

public class SearchFolderTestCommandHandler : IRequestHandler<SearchFolderTest, PaginatedList<SearchFolderTestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public SearchFolderTestCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<PaginatedList<SearchFolderTestDto>> Handle(SearchFolderTest rq, CancellationToken cancellationToken)
    {
        var authors = await _context.FolderUsers
            .Include(fu => fu.Folder)
            .ThenInclude(f => f!.CreatedByUser) 
            .Where(fu => fu.UserId.Equals(_user.UserId))
            .Select(fu => new
            {
                Id = fu.Folder!.Id,
                FullName = fu.Folder.CreatedByUser!.FullName
            })
            .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

        var folders = _context.FolderUsers
            .Include(fu => fu.Folder)
            .Where(fu => fu.UserId == _user.UserId)
            .Select(fu => new { Folder = fu.Folder, FolderUser = fu })
            .Distinct();
        
        if (!string.IsNullOrEmpty(rq.SharedMode) && Enum.TryParse<FolderShareMode>(rq.SharedMode, out var shareMode))
        {
            folders = folders.Where(f => f.FolderUser!.ShareMode == shareMode);
        }

        if (!string.IsNullOrEmpty(rq.FolderName))
        {
            folders = folders.Where(f => f.Folder!.Name.Contains(rq.FolderName));
        }

        return await PaginatedList<SearchFolderTestDto>.CreateAsync(
                folders.Select(f => new SearchFolderTestDto
                {
                    FolderTestId = f.Folder!.Id,
                    Name = f.Folder.Name,
                    SharedBy = authors.ContainsKey(f.Folder.Id) ? authors[f.Folder.Id] : null,
                }),
                rq.PageNumber,
                rq.PageSize
            );
    }
}

