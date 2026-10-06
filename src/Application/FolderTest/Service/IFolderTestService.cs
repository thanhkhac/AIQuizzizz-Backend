using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest.Service;

public interface IFolderTestService
{
    Task<bool> CanDeleteOrEditFolderTest(Guid folderTestId, CancellationToken cancellationToken);
    Task<bool> IsOwnerOrEditor(Guid folderTestId, CancellationToken cancellationToken);
    Task TryCanUseTesTemplate (Guid testTemplateId);
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

        if (isAdmin)
         return true;
        
        var canDelete = await _context.FolderUsers
            .Where(x => x.FolderId.Equals(folderTestId) && x.UserId.Equals(_user.UserId)
                                                        && FolderShareMode.Owner == x.ShareMode)
            .FirstOrDefaultAsync(cancellationToken);
        
        return canDelete != null;
    }

    public async Task<bool> IsOwnerOrEditor(Guid folderTestId, CancellationToken cancellationToken)
    {
        var folderUser = await _context.FolderUsers
            .Where(x => x.FolderId.Equals(folderTestId)
                        && x.UserId.Equals(_user.UserId)
                        && (FolderShareMode.Owner == x.ShareMode
                        || FolderShareMode.Editable == x.ShareMode))
            .FirstOrDefaultAsync(cancellationToken);

        return folderUser != null;
    }

    public async Task TryCanUseTesTemplate(Guid testTemplateId)
    {
        // Thêm template vào folder = chia sẻ lại cho thành viên folder -> chỉ Owner/Editable (ViewOnly không được)
        var accessToView = await _context.TestTemplateUsers
            .Where(t => t.UserId.Equals(_user.UserId) && t.TestTemplateId.Equals(testTemplateId)
                        && (t.ShareMode == TestTemplateUserShareMode.Owner || t.ShareMode == TestTemplateUserShareMode.Editable))
            .FirstOrDefaultAsync();
        
        if (accessToView == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE, "User không có quyền dùng test template này");
    }
}
