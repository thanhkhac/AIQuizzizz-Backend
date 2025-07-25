using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest;

[Authorize]
public class GetSharingInFolderQuery : IRequest<ResourceShareDto>
{
    public Guid FolderId { get; set; }
}

public class GetSharingInFolderQueryValidator : AbstractValidator<GetSharingInFolderQuery>
{
    public GetSharingInFolderQueryValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được trống");
    }
}

public class GetSharingInFolderQueryHandler : IRequestHandler<GetSharingInFolderQuery, ResourceShareDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IFolderTestService _folderTestService;
    
    public GetSharingInFolderQueryHandler(IApplicationDbContext context, IFolderTestService folderTestService)
    {
        _context = context;
        _folderTestService = folderTestService;
    }
    
    public async Task<ResourceShareDto> Handle(GetSharingInFolderQuery rq, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .Where(x => x.Id == rq.FolderId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (folder == null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_NOT_FOUND, "Không tìm thấy folder");

        if (!await _folderTestService.IsOwnerOrEditor(rq.FolderId, cancellationToken))
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER,
                "User không có quyền trong folder");
        
        var folderUsers = await _context.FolderUsers
            .Include(x => x.User)
            .Where(x => x.FolderId.Equals(rq.FolderId))
            .ToListAsync(cancellationToken);
        
        var ownerSharedMode = new SharingModelDto{ShareMode = "Owner", SharingUsers = new List<SharingUserDto>()};
        var editableSharedMode = new SharingModelDto{ShareMode = "Editable", SharingUsers = new List<SharingUserDto>()};
        var viewSharedMode = new SharingModelDto{ShareMode = "ViewOnly", SharingUsers = new List<SharingUserDto>()};

        folderUsers.ForEach(x =>
        {
            switch (x.ShareMode)
            {
                case FolderShareMode.Owner:
                    ownerSharedMode.SharingUsers.Add(new SharingUserDto
                    {
                        UserId = x.User!.Id, FullName = x.User!.FullName
                    });
                    break;
                case FolderShareMode.Editable:
                    editableSharedMode.SharingUsers.Add(new SharingUserDto
                    {
                        UserId = x.User!.Id, FullName = x.User!.FullName
                    });
                    break;
                case FolderShareMode.ViewOnly:
                    viewSharedMode.SharingUsers.Add(new SharingUserDto
                    {
                        UserId = x.User!.Id, FullName = x.User!.FullName
                    });
                    break;
                default:
                    throw new ErrorCodeException(ErrorCodes.INVALID_SHARE_MODE,
                        $"Chế độ chia sẻ không hợp lệ: {x.ShareMode}");
            }
        });

        var sharingModelDtos = new List<SharingModelDto> 
        { 
            ownerSharedMode, 
            editableSharedMode, 
            viewSharedMode 
        };

        return new ResourceShareDto
        {
            Id = rq.FolderId,
            SharingModel = sharingModelDtos
        };
    }
}
