using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Users;

[Authorize (Roles = Domain.Constants.Roles.Administrator)]
public class ChangeAccountRoleCommand : IRequest<Guid>
{
    /// <summary>
    /// Id of the user want to change role
    /// </summary>   
    public required Guid UserId { get; set; }
    public required string Role { get; set; }
    
}

public class ChangeAccountRoleCommandValidator : AbstractValidator<ChangeAccountRoleCommand>
{
    public ChangeAccountRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được trống");
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role không được trống");
    }
}

public class ChangeAccountRoleCommandHandler : IRequestHandler<ChangeAccountRoleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    
    public ChangeAccountRoleCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    
    /// <summary>
    /// The function changes the role of a user account and returns the user ID
    /// </summary>
    /// <param name="rq">Request contains UserId and Role information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<Guid> Handle(ChangeAccountRoleCommand rq, CancellationToken cancellationToken)
    {
        var result = await _identityService.ChangeRoleAsync(rq.UserId, rq.Role);
        return result;
    }
}
