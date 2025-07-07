using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Users;

[Authorize (Roles = Roles.Administrator)]
public class ActiveAccountCommand : IRequest<Guid>
{
    public required Guid UserId { get; set; }
}

public class ActiveAccountCommandValidator : AbstractValidator<ActiveAccountCommand>
{
    public ActiveAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được trống");
    }
}

public class ActiveAccountCommandHandler : IRequestHandler<ActiveAccountCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    
    public ActiveAccountCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    
    public async Task<Guid> Handle(ActiveAccountCommand rq, CancellationToken cancellationToken)
    {
        var admins = await _identityService.GetUsersInRoleAsync();
        
        var bannedUsers = await _context.DomainUsers
            .Where(x => x.IsDeleted == false 
                        && x.Id == rq.UserId 
                        && x.IsDeleted == false
                        && !admins.Contains(x.Id))
            .FirstOrDefaultAsync(cancellationToken);
        if (bannedUsers == null)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with id {_user.UserId} not found");

        bannedUsers.IsBanned = false;
        await _identityService.ActiveUser(rq.UserId);
        await _context.SaveChangesAsync(cancellationToken);
        return bannedUsers.Id;
    }
}
