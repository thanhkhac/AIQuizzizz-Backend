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
    public List<UpsertSharingModelDto> SharingModels { get; set; } = new();
    public List<Guid> DeleteUserIds { get; set; } = new();
}

public class UpdateSharingInFolderCommandValidator : AbstractValidator<UpdateSharingInFolderCommand>
{
    public UpdateSharingInFolderCommandValidator()
    {
        RuleFor(x => x.FolderId)
            .NotEmpty().WithMessage("FolderId không được trống");

        RuleFor(x => x.SharingModels)
            .NotNull().WithMessage("SharingModels is required.")
            .ForEach(model =>
            {
                model.SetValidator(new UpsertSharingModelDtoValidator());
            });
    }
}

public class UpsertSharingModelDtoValidator : AbstractValidator<UpsertSharingModelDto>
{
    public static readonly string[] AllowedModes = new[]
    {
        "Editable", "ViewOnly"
    };

    public UpsertSharingModelDtoValidator()
    {
        RuleFor(x => x.SharingUserId)
            .NotNull().WithMessage("SharingUserId is required.");

        RuleFor(x => x.ShareMode)
            .NotEmpty().WithMessage("ShareMode is required.")
            .Must(mode => AllowedModes.Contains(mode))
            .WithMessage("ShareMode must be either 'Editable' or 'ViewOnly'.");
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
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER, "User không có quyền trong folder");


        var ownerId = folder.CreatedBy!.Value;
        var sharingModels = rq.SharingModels;
        sharingModels.RemoveAll(x => x.SharingUserId == ownerId);
        var sharingModelIds = sharingModels.Select(x => x.SharingUserId).ToList();

        var existingUserIds = await _context.DomainUsers
            .Where(u => sharingModelIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var invalidUserIds = sharingModelIds.Except(existingUserIds).ToList();


        var existedQuestionSetUser =
            await _context.FolderUsers
                .Where(x => x.FolderId == rq.FolderId && sharingModelIds.Contains(x.UserId))
                .ToListAsync(cancellationToken);


        foreach (var model in sharingModels)
        {
            var entity = existedQuestionSetUser.Find(x => x.UserId == model.SharingUserId);
            var newShareMode = Enum.Parse<FolderShareMode>(model.ShareMode!);
            //Đã tồn tại
            if (entity != null)
            {
                if (entity.ShareMode == FolderShareMode.Owner)
                    continue;
                entity.ShareMode = newShareMode;
            }
            else //chưa tồn tại
            {
                var newFolderUser = new FolderUser
                {
                    FolderId = rq.FolderId,
                    UserId = model.SharingUserId,
                    ShareMode = newShareMode
                };
                _context.FolderUsers.Add(newFolderUser);
            }
        }


        var deleteIds = rq.DeleteUserIds;
        deleteIds.Remove(ownerId);

        var deleteEntities = _context.FolderUsers
            .Where(x => deleteIds.Contains(x.UserId)
                        && x.FolderId == rq.FolderId);

        _context.FolderUsers.RemoveRange(deleteEntities);
        await _context.SaveChangesAsync(cancellationToken);

        return rq.FolderId;
    }
}
