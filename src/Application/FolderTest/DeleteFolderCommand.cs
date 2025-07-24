using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.FolderTest;

[Authorize]
public class DeleteFolderCommand : IRequest<Guid>
{
    public required Guid FolderId { get; set; }
}

public class DeleteFolderCommandValidator : AbstractValidator<DeleteFolderCommand>
{
    public DeleteFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được để trống");
    }
}

public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFolderTestService _folderTestService;
    
    public DeleteFolderCommandHandler(IApplicationDbContext context, IFolderTestService folderTestService)
    {
        _context = context;
        _folderTestService = folderTestService;
    }
    
    public async Task<Guid> Handle(DeleteFolderCommand rq, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders.Where(x => x.Id == rq.FolderId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (folder == null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_NOT_FOUND, "Không tìm thấy folder");
        
        var canDelete = await _folderTestService.CanDeleteOrEditFolderTest(rq.FolderId, cancellationToken);
        if(!canDelete)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER,
                "User không có quyền xóa folder");
        
        folder.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return folder.Id;
    }
}
