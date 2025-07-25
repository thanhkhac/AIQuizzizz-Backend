using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.TestTemplates.Service;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TestTemplates;

[Authorize]
public class AddSharingTestTemplateCommand : IRequest<Guid>
{
    public Guid TestTemplateId { get; set; }
    public UpsertSharing? Sharing { get; set; }
}

public class AddSharingTestTemplateCommandValidator : AbstractValidator<AddSharingTestTemplateCommand>
{
    public AddSharingTestTemplateCommandValidator()
    {
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("FolderId không được trống");

        RuleFor(x => x.Sharing)
            .Must(x => x == null || x.SharingModel
                .All(x => new[] { "Editable", "ViewOnly" }.Contains(x.ShareMode)))
            .WithMessage("SharedMode phải là Editable, ViewOnly");
    }
}

public class AddSharingTestTemplateCommandHandler : IRequestHandler<AddSharingTestTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestTemplateService _testTemplateService;
    
    public AddSharingTestTemplateCommandHandler(IApplicationDbContext context, ITestTemplateService testTemplateService)
    {
        _context = context;
        _testTemplateService = testTemplateService;
    }
    
    public async Task<Guid> Handle(AddSharingTestTemplateCommand rq, CancellationToken cancellationToken)
    {
        var testTemplate = await _testTemplateService.CanEditTestTemplate(rq.TestTemplateId, cancellationToken);
        
        if (rq.Sharing == null || rq.Sharing.SharingModel.Count <= 0)
            return rq.TestTemplateId;
        
        var userIds = rq.Sharing.SharingModel
            .Select(x => x.SharingUserId)
            .ToList();
        
        var users = _context.DomainUsers
            .Where(x => userIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x);
        
        var userTemplateExits = await _context.TestTemplateUsers
            .Where(x => x.TestTemplateId.Equals(rq.TestTemplateId))
            .ToDictionaryAsync(x => x.UserId, x => x, cancellationToken);

        var userTemplates = new List<TestTemplateUser>();
        
        rq.Sharing.SharingModel.ForEach(x =>
        {
            if (x.SharingUserId == null || x.ShareMode == null) {
                return;
            }
            var shareMode = Enum.Parse<TestTemplateUserShareMode>(x.ShareMode!);
                
            if (!users.ContainsKey(x.SharingUserId.Value))
                throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND,
                    $"UserId {x.SharingUserId.Value} không tồn tại");
                            
            if (userTemplateExits.ContainsKey(x.SharingUserId.Value))
                throw new ErrorCodeException(ErrorCodes.USER_ALREADY_EXISTS_IN_FOLDER,
                    $"UserId {x.SharingUserId.Value} đã có trong folder");
                            
            var userFolder = new TestTemplateUser
            {
                TestTemplateId = rq.TestTemplateId, UserId = x.SharingUserId.Value, ShareMode = shareMode
            };
            userTemplates.Add(userFolder);
        });
        
        _context.TestTemplateUsers.AddRange(userTemplates);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return rq.TestTemplateId;
    }
}
