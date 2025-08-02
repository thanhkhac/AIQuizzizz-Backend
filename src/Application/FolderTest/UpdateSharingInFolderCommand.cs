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

        //Xử lý lấy ra những Id 
        var sharingModelDict = rq.SharingModels
            .Where(x => x.SharingUserId.HasValue)
            .GroupBy(x => x.SharingUserId!.Value)
            .ToDictionary(g => g.Key, g => g.Last().ShareMode);

        var existingUserIds = await _context.DomainUsers
            .Where(u => sharingModelDict.Select(x => x.Key).Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var invalidUserIds = sharingModelDict.Keys.Except(existingUserIds).ToList();

        if (invalidUserIds.Any())
            throw new ErrorCodeException(ErrorCodes.USER_NOTFOUND, $"UserId không tồn tại: {string.Join(", ", invalidUserIds)}");

        var existingFolderUsers = await _context.FolderUsers.Where(x => sharingModelDict.ContainsKey(x.UserId)).ToListAsync(cancellationToken);

        foreach (var model in sharingModelDict)
        {
            var entity = existingFolderUsers.Find(x => x.UserId == model.Key);
            //Đã tồn tại
            if (entity != null)
            {
                entity.ShareMode = Enum.Parse<FolderShareMode>(model.Value!);
            }
            else //chưa tồn tại
            {
                var newFolderUser = new FolderUser
                {
                    FolderId = rq.FolderId,
                    UserId = model.Key,
                    ShareMode = Enum.Parse<FolderShareMode>(model.Value!)
                };
                _context.FolderUsers.Add(newFolderUser);
            }
        }

        var deleteIds = rq.DeleteUserIds;
        if (deleteIds.Count > 0)
        {
            if (rq.DeleteUserIds.Contains(folder.CreatedBy!.Value))
                throw new ErrorCodeException(ErrorCodes.CAN_NOT_DELETE_OWNER, "Không thể xóa owner");

            var deleteEntities = _context.FolderUsers
                .Where(x => deleteIds.Contains(x.UserId)
                            && x.FolderId == rq.FolderId);
            ;

            _context.FolderUsers.RemoveRange(deleteEntities);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return rq.FolderId;
    }
}
