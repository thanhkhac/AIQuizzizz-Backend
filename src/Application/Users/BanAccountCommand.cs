using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Users;

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
        
        var user = await _context.DomainUsers
            .Where(x => x.Id == _user.UserId && x.IsDeleted == false && x.IsBanned == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");
        
        var bannedUsers = await _context.DomainUsers
            .Where(x => x.IsDeleted == false 
                        && x.Id == rq.UserId 
                        && x.IsDeleted == false
                        && !admins.Contains(x.Id))
            .FirstOrDefaultAsync(cancellationToken);
        if (bannedUsers == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, $"User with id {_user.UserId} not found");

        bannedUsers.IsBanned = true;
        await _context.SaveChangesAsync(cancellationToken);
        return bannedUsers.Id;
    }
}
