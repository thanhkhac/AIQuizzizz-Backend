using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TestTemplates;

public class TestTemplatePermissionsDto
{
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}

[Authorize]
public class GetTestTemplatePermissionsQuery : IRequest<TestTemplatePermissionsDto>
{
    public required Guid TestTemplateId { get; set; }
}

public class GetTestTemplatePermissionsQueryValidator : AbstractValidator<GetTestTemplatePermissionsQuery>
{
    public GetTestTemplatePermissionsQueryValidator()
    {
        RuleFor(x => x.TestTemplateId)
            .NotEmpty().WithMessage("TestTemplateId không được trống");
    }
}

public class GetTestTemplatePermissionsQueryHandler : IRequestHandler<GetTestTemplatePermissionsQuery, TestTemplatePermissionsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    
    public GetTestTemplatePermissionsQueryHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    
    /// <summary>
    /// The function retrieves the permissions for a test template for the current user, indicating if they can edit or delete it
    /// </summary>
    /// <param name="rq">Request contains TestTemplateId information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<TestTemplatePermissionsDto> Handle(GetTestTemplatePermissionsQuery rq, CancellationToken cancellationToken)
    {
        var testTemplate = await _context.TestTemplates
            .Where(t => t.Id == rq.TestTemplateId && t.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (testTemplate == null)
            throw new ErrorCodeException(ErrorCodes.TEST_TEMPLATE_NOT_FOUND, "Không tìm thất test template");
        
        if (_user.UserId == null)
            return new TestTemplatePermissionsDto
            {
                CanEdit = false,
                CanDelete = false
            };
        
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        if (isAdmin)
        {
            return new TestTemplatePermissionsDto
            {
                CanEdit = true,
                CanDelete = true
            };
        }
        
        var shareMode = await _context.TestTemplateUsers
            .Where(x => x.TestTemplateId == rq.TestTemplateId && x.UserId == _user.UserId)
            .Select(x => x.ShareMode)
            .FirstOrDefaultAsync(cancellationToken);
        
            return new TestTemplatePermissionsDto
            {
                CanEdit = shareMode == TestTemplateUserShareMode.Owner || shareMode == TestTemplateUserShareMode.Editable,
                CanDelete = shareMode == TestTemplateUserShareMode.Owner
            };
        }
    }

