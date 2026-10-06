using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest;
[Authorize]
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
            .NotEmpty().WithMessage("Name không được để trống")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Name không được để trống")
            .MaximumLength(200).WithMessage("Tên folder không được vượt quá 200 ký tự");
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
        
        var newName = rq.Name.Trim();

        // Không cho trùng tên (không phân biệt hoa thường) với folder khác của cùng chủ sở hữu (loại trừ chính nó)
        var ownerId = await _context.FolderUsers
            .Where(x => x.FolderId == folder.Id && x.ShareMode == FolderShareMode.Owner)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        var lowerName = newName.ToLower();
        var duplicated = await _context.FolderUsers
            .AnyAsync(x => x.UserId == ownerId
                           && x.ShareMode == FolderShareMode.Owner
                           && x.FolderId != folder.Id
                           && x.Folder != null
                           && !x.Folder.IsDeleted
                           && x.Folder.Name.ToLower() == lowerName, cancellationToken);
        if (duplicated)
            throw new ErrorCodeException(ErrorCodes.FOLDER_ALREADY_EXISTS, "Folder đã tồn tại");

        folder.Name = newName;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return folder.Id;
    }
}
