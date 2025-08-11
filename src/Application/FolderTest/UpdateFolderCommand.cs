using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.FolderTest;

public class UpdateFolderCommand : IRequest<Guid>
{
    public Guid? FolderId { get; set; }
    public required string Name { get; set; }
}

public class UpdateFolderCommandValidator : AbstractValidator<UpdateFolderCommand>
{
    public UpdateFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được để trống");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name không được để trống");
    }
}

public class UpdateFolderCommandHandler : IRequestHandler<UpdateFolderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IFolderTestService _folderTestService;
    private readonly IUser _user;
    
    public UpdateFolderCommandHandler(IApplicationDbContext context, IFolderTestService folderTestService, IUser user)
    {
        _context = context;
        _folderTestService = folderTestService;
        _user = user;       
    }
    
    public async Task<Guid> Handle(UpdateFolderCommand rq, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders.Where(x => x.Id == rq.FolderId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (folder == null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_NOT_FOUND, "Không tìm thấy folder");
        
        var canDelete = await _folderTestService.IsOwnerOrEditor(rq.FolderId!.Value, cancellationToken);
        if(!canDelete)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER,
                "User không có quyền edit folder");
        
        var folderByName = await _context.Folders
            .FirstOrDefaultAsync(x => x.Name == rq.Name &&
                                      x.IsDeleted == false &&
                                      x.CreatedBy == _user.UserId, cancellationToken);
        if (folderByName != null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_ALREADY_EXISTS, "Folder đã tồn tại");
        
        folder.Name = rq.Name;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return folder.Id;
    }
}
