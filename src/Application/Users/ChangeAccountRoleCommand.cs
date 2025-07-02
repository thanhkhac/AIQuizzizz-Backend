using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using StackExchange.Redis;

namespace CleanArchitectureBase.Application.Users;

[Authorize (Roles = Roles.Administrator)]
public class ChangeAccountRoleCommand : IRequest<Guid>
{
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
    
    public async Task<Guid> Handle(ChangeAccountRoleCommand rq, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers
            .Where(x => x.Id == _user.UserId && x.IsDeleted == false && x.IsBanned == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");

        var result = await _identityService.ChangeRoleAsync(rq.UserId, rq.Role);
        return result;
    }
}
