using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Accounts;

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
    
    public async Task<Guid> Handle(ActiveAccountCommand request, CancellationToken cancellationToken)
    {
        var admins = await _identityService.GetUsersInRoleAsync();
        
        var user = await _context.DomainUsers
            .Where(x => x.Id == _user.UserId && x.IsDeleted == false && x.IsBanned == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");
        
        var bannedUsers = await _context.DomainUsers
            .Where(x => x.IsDeleted == false 
                        && x.Id == user.Id 
                        && x.IsDeleted == false && x.IsBanned == true
                        && !admins.Contains(x.Id))
            .FirstOrDefaultAsync(cancellationToken);
        if (bannedUsers == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");

        bannedUsers.IsBanned = false;
        await _context.SaveChangesAsync(cancellationToken);
        return bannedUsers.Id;
    }
}
