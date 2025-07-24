using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest.Service;

public interface IFolderTestService
{
    Task<bool> CanDeleteOrEditFolderTest(Guid folderTestId, CancellationToken cancellationToken);
    Task<bool> InSharedFolder(Guid folderTestId, CancellationToken cancellationToken);
    Task<bool> IsOwner(Guid folderTestId, CancellationToken cancellationToken);
}

public class FolderTestService : IFolderTestService
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    
    public FolderTestService(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    
    public async Task<bool> CanDeleteOrEditFolderTest(Guid folderTestId, CancellationToken cancellationToken)
    {
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
            Domain.Constants.Roles.Moderator);

        if (!isAdmin)
        {
            var canDelete = await _context.FolderUsers
                .Where(x => x.FolderId.Equals(folderTestId) && x.UserId.Equals(_user.UserId)
                                                            && FolderShareMode.Owner == x.ShareMode)
                .FirstOrDefaultAsync(cancellationToken);

            if (canDelete == null)
                return false;
        }
        return true;
    }

    public async Task<bool> InSharedFolder(Guid folderTestId, CancellationToken cancellationToken)
    {
        var folderUser = await _context.FolderUsers
            .Where(x => x.FolderId.Equals(folderTestId) && x.UserId.Equals(_user.UserId))
            .FirstOrDefaultAsync(cancellationToken);
        if (folderUser == null)
            return false;

        return true;
    }

    public async Task<bool> IsOwner(Guid folderTestId, CancellationToken cancellationToken)
    {
        var folderUser = await _context.FolderUsers
            .Where(x => x.FolderId.Equals(folderTestId)
                        && x.UserId.Equals(_user.UserId)
                        && FolderShareMode.Owner == x.ShareMode)
            .FirstOrDefaultAsync(cancellationToken);
        if (folderUser == null)
            return false;

        return true;
    }
}
