using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.Common;

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
    
    public async Task<Guid> Handle(CreateFolderCommand rq, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .FirstOrDefaultAsync(x => x.Name == rq.FolderName &&
                                      x.IsDeleted == false &&
                                      x.CreatedBy == _user.UserId, cancellationToken);
        if (folder != null)
            throw new ErrorCodeException(ErrorCodes.FOLDER_ALREADY_EXISTS, "Folder đã tồn tại");

        var newFolder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = rq.FolderName
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
