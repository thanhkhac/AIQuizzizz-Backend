using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest;

[Authorize]
public class UpdateSharingInFolderCommand : IRequest<Guid>
{
    public Guid FolderId { get; set; }
    public UpsertSharing? UpdateSharing { get; set; }
    public List<Guid>? DeleteUserIds { get; set; } = new();
}

public class UpdateSharingInFolderCommandValidator : AbstractValidator<UpdateSharingInFolderCommand>
{
    public UpdateSharingInFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được trống");
        
        RuleFor(x => x.UpdateSharing)
            .Must(x => x == null || x.SharingModel
                .All(x => new[] { "Editable", "ViewOnly" }.Contains(x.ShareMode)))
            .WithMessage("SharedMode phải là Editable, ViewOnly");
    }
}

public class UpdateSharingInFolderCommandHandler : IRequestHandler<UpdateSharingInFolderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFolderTestService _folderTestService;
    
    public UpdateSharingInFolderCommandHandler(IApplicationDbContext context, IFolderTestService folderTestService)
    {
        _context = context;
        _folderTestService = folderTestService;
    }
    
    public async Task<Guid> Handle(UpdateSharingInFolderCommand rq, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .Where(x => x.Id == rq.FolderId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (folder == null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_NOT_FOUND, "Không tìm thấy folder");

        if (!await _folderTestService.IsOwnerOrEditor(rq.FolderId, cancellationToken))
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER,
                "User không có quyền trong folder");
        
        var userFolders = await _context.FolderUsers
            .Where(x => x.FolderId.Equals(rq.FolderId))
            .ToDictionaryAsync(x => x.UserId, x => x, cancellationToken);

        if (rq.UpdateSharing != null)
        {
            rq.UpdateSharing.SharingModel.ForEach(x =>
            {
                if (x.SharingUserId == null || x.ShareMode == null) {
                    return;
                }
                var shareMode = Enum.Parse<FolderShareMode>(x.ShareMode!);
                
                if (!userFolders.ContainsKey(x.SharingUserId.Value))
                    throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND,
                        $"UserId {x.SharingUserId.Value} không tồn tại trong folder");
                
                userFolders[x.SharingUserId.Value].ShareMode = shareMode;
            });
        }
        
        if (rq.DeleteUserIds?.Count > 0)
        {
            if (rq.DeleteUserIds.Contains(folder.CreatedBy!.Value))
                throw new ErrorCodeException(ErrorCodes.CAN_NOT_DELETE_OWNER, "Không thể xóa owner");
            
            var deleteUserIds = rq.DeleteUserIds
                .Where(id => userFolders.ContainsKey(id))
                .Select(id => userFolders[id])
                .ToList();

            _context.FolderUsers.RemoveRange(deleteUserIds);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return rq.FolderId;
    }
}
