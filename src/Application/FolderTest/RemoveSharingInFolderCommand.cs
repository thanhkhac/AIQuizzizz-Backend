using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.FolderTest;

public class RemoveSharingInFolderCommand : IRequest<Guid>
{
    public Guid FolderId { get; set; }
    public List<Guid> UserIds { get; set; } = new();
}

public class RemoveSharingInFolderCommandValidator : AbstractValidator<RemoveSharingInFolderCommand>
{
    public RemoveSharingInFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được trống");
        
        RuleFor(x => x.UserIds)
            .Must(q => q != null && q.Count > 0)
            .WithMessage("Số lượng id xóa lớn hơn 0");
    }
}

public class RemoveSharingInFolderCommandHandler : IRequestHandler<RemoveSharingInFolderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFolderTestService _folderTestService;
    
    public RemoveSharingInFolderCommandHandler(IApplicationDbContext context, IFolderTestService folderTestService)
    {
        _context = context;
        _folderTestService = folderTestService;
    }
    
    public async Task<Guid> Handle(RemoveSharingInFolderCommand rq, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .Where(x => x.Id == rq.FolderId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (folder == null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_NOT_FOUND, "Không tìm thấy folder");

        if (!await _folderTestService.IsOwner(rq.FolderId, cancellationToken))
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER,
                "User không có quyền trong folder");
        
        var userFolders = await _context.FolderUsers
            .Where(x => x.FolderId.Equals(rq.FolderId)
                        && rq.UserIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        if (userFolders.Count > 0)
        {
            _context.FolderUsers.RemoveRange(userFolders);
            await _context.SaveChangesAsync(cancellationToken);
        }
        return rq.FolderId;
    }
}
