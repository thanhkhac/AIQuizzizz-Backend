using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Users;

[Authorize(Roles = Domain.Constants.Roles.Administrator)]
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
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role không được trống")
            .Must(role => role == Domain.Constants.Roles.User || role == Domain.Constants.Roles.Moderator)
            .WithMessage("Role không hợp lệ. Chỉ được phép: 'Moderator' hoặc 'User'");
    }
}

public class ChangeAccountRoleCommandHandler : IRequestHandler<ChangeAccountRoleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public ChangeAccountRoleCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    /// <summary>
    /// The function changes the role of a user account and returns the user ID
    /// </summary>
    /// <param name="rq">Request contains UserId and Role information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<Guid> Handle(ChangeAccountRoleCommand rq, CancellationToken cancellationToken)
    {
        var result = await _identityService.ChangeRoleAsync(rq.UserId, rq.Role, new List<string>
        {
            Domain.Constants.Roles.Administrator
        });
        return result;
    }
}
