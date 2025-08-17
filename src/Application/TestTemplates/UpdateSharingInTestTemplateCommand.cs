using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.TestTemplates.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TestTemplates;

[Authorize]
public class UpdateSharingInTestTemplateCommand : IRequest<Guid>
{
    public Guid TestTemplateId { get; set; }
    public List<UpsertSharingModelDto>? SharingModels { get; set; } = new();
    public List<Guid> DeleteUserIds { get; set; } = new();
}

public class UpdateSharingInTestTemplateCommandValidator : AbstractValidator<UpdateSharingInTestTemplateCommand>
{
    public UpdateSharingInTestTemplateCommandValidator()
    {
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("FolderId không được trống");

        RuleFor(x => x.SharingModels)
            .NotNull()
            .Must(x => x == null || x
                .All(x => new[]
                {
                    "Editable", "ViewOnly"
                }.Contains(x.ShareMode)))
            .WithMessage("SharedMode phải là Editable, ViewOnly");
    }
}

public class UpdateSharingInTestTemplateCommandHandler : IRequestHandler<UpdateSharingInTestTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestTemplateService _testTemplateService;

    public UpdateSharingInTestTemplateCommandHandler(IApplicationDbContext context, ITestTemplateService testTemplateService)
    {
        _context = context;
        _testTemplateService = testTemplateService;
    }

    public async Task<Guid> Handle(UpdateSharingInTestTemplateCommand rq, CancellationToken cancellationToken)
    {
        //Kiểm tra tồn tại
        var testTemplate = await _context.TestTemplates
            .Where(t => t.Id == rq.TestTemplateId && t.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);

        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND);

        //Kiểm tra quyền
        var canUpdateSharing = await _testTemplateService.CanEditTestTemplate(rq.TestTemplateId, cancellationToken);
        if (!canUpdateSharing)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE, "Không có quyền sửa");

        var userTemplateExits = await _context.TestTemplateUsers
            .Where(x => x.TestTemplateId.Equals(rq.TestTemplateId))
            .ToDictionaryAsync(x => x.UserId, x => x, cancellationToken);


        var ownerId = testTemplate.CreatedBy!.Value;
        var sharingModels = rq.SharingModels;
        sharingModels!.RemoveAll(x => x.SharingUserId == ownerId);
        var sharingModelIds = sharingModels.Select(x => x.SharingUserId).ToList();

        var existingUserIds = await _context.DomainUsers
            .Where(u => sharingModelIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var invalidUserIds = sharingModelIds.Except(existingUserIds).ToList();

        if (invalidUserIds.Any())
            throw new ErrorCodeException(ErrorCodes.USER_NOTFOUND, $"UserId không tồn tại: {string.Join(", ", invalidUserIds)}");

        var existedTesTemplateUser =
            await _context.TestTemplateUsers
                .Where(x => x.TestTemplateId == rq.TestTemplateId && sharingModelIds.Contains(x.UserId))
                .ToListAsync(cancellationToken);


        foreach (var model in sharingModels)
        {
            var entity = existedTesTemplateUser.Find(x => x.UserId == model.SharingUserId);
            var newShareMode = Enum.Parse<TestTemplateUserShareMode>(model.ShareMode!);

            if (entity != null)
            {
                if (entity.ShareMode == TestTemplateUserShareMode.Owner)
                    continue;
                entity.ShareMode = newShareMode;
            }
            else
            {
                var newTestTemplateUser = new TestTemplateUser()
                {
                    TestTemplateId = rq.TestTemplateId,
                    UserId = model.SharingUserId,
                    ShareMode = newShareMode
                };
                _context.TestTemplateUsers.Add(newTestTemplateUser);
            }
        }

        var deleteIds = rq.DeleteUserIds;
        deleteIds.Remove(ownerId);


        var deleteEntities = _context.TestTemplateUsers
            .Where(x => deleteIds.Contains(x.UserId)
                        && x.TestTemplateId == rq.TestTemplateId);


        _context.TestTemplateUsers.RemoveRange(deleteEntities);

        await _context.SaveChangesAsync(cancellationToken);

        return rq.TestTemplateId;
    }
}
