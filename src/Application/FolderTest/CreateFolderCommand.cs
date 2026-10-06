using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.FolderTest;

[Authorize]
public class CreateFolderCommand : IRequest<Guid>
{
    public required string FolderName { get; set; }
}

public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator()
    {
        RuleFor(x => x.FolderName)
            .NotEmpty().WithMessage("Tên folder không được để trống")
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Tên folder không được để trống")
            .MaximumLength(200).WithMessage("Tên folder không được vượt quá 200 ký tự");
    }
}

public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, Guid>
{
    public readonly IApplicationDbContext _context;
    public readonly IUser _user;
    
    public CreateFolderCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    /// <summary>
    /// The function creates a new folder and assigns the creator as the owner, returning the folder ID
    /// </summary>
    /// <param name="rq">Request contains FolderName information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<Guid> Handle(CreateFolderCommand rq, CancellationToken cancellationToken)
    {
        var folderName = rq.FolderName.Trim();
        var userId = _user.UserId ?? throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with id {_user.UserId} not found");

        // Không cho trùng tên (không phân biệt hoa thường) với folder khác do cùng user sở hữu
        var lowerName = folderName.ToLower();
        var duplicated = await _context.FolderUsers
            .AnyAsync(x => x.UserId == userId
                           && x.ShareMode == FolderShareMode.Owner
                           && x.Folder != null
                           && !x.Folder.IsDeleted
                           && x.Folder.Name.ToLower() == lowerName, cancellationToken);
        if (duplicated)
            throw new ErrorCodeException(ErrorCodes.FOLDER_ALREADY_EXISTS, "Folder đã tồn tại");

        var newFolder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = folderName
        };

        var folderUser = new FolderUser
        {
            UserId = _user.UserId ?? throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with id {_user.UserId} not found"),
            FolderId = newFolder.Id,
            ShareMode = FolderShareMode.Owner
        };
        
        _context.Folders.Add(newFolder);
        _context.FolderUsers.Add(folderUser);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return newFolder.Id;
    }
}
