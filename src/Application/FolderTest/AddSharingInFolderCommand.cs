using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest;

[Authorize]
public class AddSharingInFolderCommand : IRequest<Guid>
{
    public Guid FolderId { get; set;}
    public UpsertSharing? Sharing { get; set; }
}

public class AddSharingInFolderCommandValidator : AbstractValidator<AddSharingInFolderCommand>
{
    public AddSharingInFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được trống");

        RuleFor(x => x.Sharing)
            .Must(x => x == null || x.SharingModel
                .All(x => new[] { "Editable", "ViewOnly" }.Contains(x.ShareMode)))
            .WithMessage("SharedMode phải là Editable, ViewOnly");
    }
}

public class AddSharingInFolderCommandHandler : IRequestHandler<AddSharingInFolderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFolderTestService _folderTestService;
    
    public AddSharingInFolderCommandHandler(IApplicationDbContext context, IFolderTestService folderTestService)
    {
        _context = context;
        _folderTestService = folderTestService;
    }
    
    public async Task<Guid> Handle(AddSharingInFolderCommand rq, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .Where(x => x.Id == rq.FolderId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (folder == null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_NOT_FOUND, "Không tìm thấy folder");

        if (!await _folderTestService.IsOwnerOrEditor(rq.FolderId, cancellationToken))
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER,
                "User không có quyền trong folder");
        
        if (rq.Sharing == null || rq.Sharing.SharingModel.Count <= 0)
            return rq.FolderId;

        var userIds = rq.Sharing.SharingModel
            .Select(x => x.SharingUserId)
            .ToList();
        
        var users = _context.DomainUsers
            .Where(x => userIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);
        
        var userFoldersExits = await _context.FolderUsers
            .Where(x => x.FolderId.Equals(rq.FolderId))
            .ToDictionaryAsync(x => x.UserId, x => x, cancellationToken);
        
        var userFolders = new List<FolderUser>();
        
        rq.Sharing.SharingModel.ForEach(x =>
        {
            if (x.SharingUserId == null || x.ShareMode == null) {
                return;
            }
            var shareMode = Enum.Parse<FolderShareMode>(x.ShareMode!);
                
            if (!users.ContainsKey(x.SharingUserId.Value))
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND,
                    $"UserId {x.SharingUserId.Value} không tồn tại");
                            
            if (userFoldersExits.ContainsKey(x.SharingUserId.Value))
                throw new ErrorCodeException(ErrorCodes.USER_ALREADY_EXISTS_IN_FOLDER,
                    $"UserId {x.SharingUserId.Value} đã có trong folder");
                            
            var userFolder = new FolderUser
            {
                FolderId = rq.FolderId, UserId = x.SharingUserId.Value, ShareMode = shareMode
            };
            userFolders.Add(userFolder);
        });
        
        _context.FolderUsers.AddRange(userFolders);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return rq.FolderId;
    }
}
