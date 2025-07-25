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
        
        var sharedModes = new List<SharingModelDto>();

        folderUsers.ForEach(x =>
        {
            sharedModes.Add(new SharingModelDto
            {
                ShareMode = x.ShareMode.ToString(),
                UserId = x.User!.Id,
                FullName = x.User.FullName
            });
        });
        
        var orderPriority = new List<string> { "Owner", "Editable", "ViewOnly" };
        
        return new ResourceShareDto
        {
            Id = rq.FolderId,
            SharingModel = sharedModes
                .OrderBy(x => orderPriority.IndexOf(x.ShareMode!))
                .ToList()
        };
    }
}
