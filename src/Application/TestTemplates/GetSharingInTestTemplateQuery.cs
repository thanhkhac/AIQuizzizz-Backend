using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.TestTemplates.Service;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.TestTemplates;

[Authorize]
public class GetSharingInTestTemplateQuery : IRequest<ResourceShareDto>
{
    public required Guid TestTemplateId { get; set; }
}

public class GetSharingInTestTemplateQueryValidator : AbstractValidator<GetSharingInTestTemplateQuery>
{
    public GetSharingInTestTemplateQueryValidator()
    {
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("FolderId không được trống");
    }
}

public class GetSharingInTestTemplateQueryHandler : IRequestHandler<GetSharingInTestTemplateQuery, ResourceShareDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITestTemplateService _testTemplateService;

    public GetSharingInTestTemplateQueryHandler(IApplicationDbContext context, ITestTemplateService testTemplateService)
    {
        _context = context;
        _testTemplateService = testTemplateService;
    }
    
    public async Task<ResourceShareDto> Handle(GetSharingInTestTemplateQuery rq, CancellationToken cancellationToken)
    {
        var testTemplate = await _context.TestTemplates
            .Where(t => t.Id == rq.TestTemplateId && t.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND, "Không tìm thấy test template");
            
        var canGetSharing = await _testTemplateService.CanEditTestTemplate(rq.TestTemplateId, cancellationToken);
        if (!canGetSharing)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE, "Không có quyền xem");

        var testTemplateUsers = await _context.TestTemplateUsers
            .Include(x => x.User)
            .Where(x => x.TestTemplateId.Equals(rq.TestTemplateId))
            .ToListAsync(cancellationToken);
        
        var sharedModes = new List<SharingModelDto>();

        testTemplateUsers.ForEach(x =>
        {
            sharedModes.Add(new SharingModelDto
            {
                ShareMode = x.ShareMode.ToString(),
                UserId = x.User!.Id,
                FullName = x.User.FullName
            });
        });
        
        var orderPriority = new List<string> { "Owner", "Editable", "ViewOnly" };
        
        return new ResourceShareDto
        {
            Id = rq.TestTemplateId,
            SharingModel = sharedModes
                .OrderBy(x => orderPriority.IndexOf(x.ShareMode!))
                .ToList()
        };
    }
}
