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
    public UpsertSharing? UpdateSharing { get; set; }
    public List<Guid>? DeleteUserIds { get; set; } = new();
}

public class UpdateSharingInTestTemplateCommandValidator : AbstractValidator<UpdateSharingInTestTemplateCommand>
{
    public UpdateSharingInTestTemplateCommandValidator()
    {
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("FolderId không được trống");
        
        RuleFor(x => x.UpdateSharing)
            .Must(x => x == null || x.SharingModel
                .All(x => new[] { "Editable", "ViewOnly" }.Contains(x.ShareMode)))
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
        var testTemplate = await _testTemplateService.CanEditTestTemplate(rq.TestTemplateId, cancellationToken);

        var userTemplateExits = await _context.TestTemplateUsers
            .Where(x => x.TestTemplateId.Equals(rq.TestTemplateId))
            .ToDictionaryAsync(x => x.UserId, x => x, cancellationToken);
        
        if (rq.UpdateSharing != null)
        {
            rq.UpdateSharing.SharingModel.ForEach(x =>
            {
                if (x.SharingUserId == null || x.ShareMode == null) {
                    return;
                }
                var shareMode = Enum.Parse<TestTemplateUserShareMode>(x.ShareMode!);
                
                if (!userTemplateExits.ContainsKey(x.SharingUserId.Value))
                    throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND,
                        $"UserId {x.SharingUserId.Value} không tồn tại trong folder");
                
                userTemplateExits[x.SharingUserId.Value].ShareMode = shareMode;
            });
        }
        
        if (rq.DeleteUserIds?.Count > 0)
        {
            if (rq.DeleteUserIds.Contains(testTemplate.CreatedBy!.Value))
                throw new ErrorCodeException(ErrorCodes.CAN_NOT_DELETE_OWNER, "Không thể xóa owner");
            
            var deleteUserIds = rq.DeleteUserIds
                .Where(id => userTemplateExits.ContainsKey(id))
                .Select(id => userTemplateExits[id])
                .ToList();

            _context.TestTemplateUsers.RemoveRange(deleteUserIds);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return rq.TestTemplateId;
    }
}
