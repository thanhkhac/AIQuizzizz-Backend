using CleanArchitectureBase.Application.Common.Interfaces;
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
public class SearchFolderTest : IRequest<List<SearchFolderTestDto>>
{
    public string? FolderName { get; set; }
    public string? SharedMode { get; set; }
}

public class SearchFolderTestValidator : AbstractValidator<SearchFolderTest>
{
    public SearchFolderTestValidator()
    {
        RuleFor(x => x.SharedMode)
            .Must(mode => new[] {"Owner", "Editable", "ViewOnly"}.Contains(mode) || string.IsNullOrEmpty(mode))
            .WithMessage($"SharedMode phải là Owner, Editable, ViewOnly");
    }
}

public class SearchFolderTestCommandHandler : IRequestHandler<SearchFolderTest, List<SearchFolderTestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public SearchFolderTestCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<List<SearchFolderTestDto>> Handle(SearchFolderTest rq, CancellationToken cancellationToken)
    {
        var authors = await _context.Folders
            .Join(_context.FolderUsers,
                f => f.Id,
                fu => fu.FolderId,
                (f, fu) => new { Folder = f, FolderUser = fu })
            .Join(_context.DomainUsers,
                f => f.Folder.CreatedBy,
                u => u.Id,
                (f, u) => new { 
                    Id = f.Folder.Id,
                    FullName = u.FullName,
                    FolderUser = f.FolderUser })
            .Where(f => f.FolderUser.UserId.Equals(_user.UserId))
            .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);

        var folders = await _context.Folders
            .GroupJoin(_context.FolderUsers,
                folder => folder.Id,
                folderUser => folderUser.FolderId,
                (folder, folderUsers) => new { Folder = folder, FolderUsers = folderUsers })
            .SelectMany(
                x => x.FolderUsers.DefaultIfEmpty(),
                (folder, folderUser) => new { Folder = folder.Folder, FolderUser = folderUser })
            .Where(x => x.FolderUser != null && x.FolderUser.UserId == _user.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        if (!string.IsNullOrEmpty(rq.SharedMode) && Enum.TryParse<FolderShareMode>(rq.SharedMode, out var shareMode))
        {
            folders = folders.Where(f => f.FolderUser!.ShareMode == shareMode).ToList();
        }

        if (!string.IsNullOrEmpty(rq.FolderName))
        {
            folders = folders.Where(f => f.Folder.Name.Contains(rq.FolderName)).ToList();
        }

        return folders.Select(f => new SearchFolderTestDto
        {
            FolderTestId = f.Folder.Id,
            Name = f.Folder.Name,
            SharedBy = authors.ContainsKey(f.Folder.Id) ? authors[f.Folder.Id] : null,
        }).ToList();
    }
}

