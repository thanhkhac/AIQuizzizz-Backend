using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Users;

[Authorize (Roles = Roles.Administrator)]
public class BanAccountCommand : IRequest<Guid>
{
    public required Guid UserId { get; set; }
}

public class BanAccountCommandValidator : AbstractValidator<BanAccountCommand>
{
    public BanAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được trống");
    }
}

public class BanAccountCommandHandler : IRequestHandler<BanAccountCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    
    public BanAccountCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    
    public async Task<Guid> Handle(BanAccountCommand rq, CancellationToken cancellationToken)
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

        bannedUsers.IsBanned = true;
        await _identityService.BanUser(rq.UserId);
        await _context.SaveChangesAsync(cancellationToken);
        return bannedUsers.Id;
    }
}
