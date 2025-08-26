using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Users;

[Authorize(Roles = Domain.Constants.Roles.Administrator + "," + Domain.Constants.Roles.Moderator)]
public class BanAccountCommand : IRequest<Guid>
{
    /// <summary>
    /// Id of the user want to ban
    /// </summary>
    [JsonIgnore]
    public Guid UserId { get; set; }

    public bool IsBanned { get; set; }
    
    public string? Message { get; set; }
}

public class BanAccountCommandValidator : AbstractValidator<BanAccountCommand>
{
    public BanAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được trống");
            
        RuleFor(x => x.Message).MaximumLength(5000);
    }
}

public class BanAccountCommandHandler : IRequestHandler<BanAccountCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IEmailService _emailService;

    public BanAccountCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser user, IEmailService emailService)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
        _emailService = emailService;
    }

    /// <summary>
    /// The function bans a user account by setting the ban status, returning the user ID
    /// </summary>
    /// <param name="rq">Request contains UserId information</param>
    /// <param name="cancellationToken">Token to cancel the task</param>
    public async Task<Guid> Handle(BanAccountCommand rq, CancellationToken cancellationToken)
    {
        if (rq.UserId == _user.UserId)
            throw new ErrorCodeException(ErrorCodes.COMMON_FORBIDDEN, "You cannot ban your own account.");
    
        var admins = await _identityService.GetUsersInRoleAsync();

        var bannedUsers = await _context.DomainUsers
            .IgnoreQueryFilters()
            .Where(x => x.IsDeleted == false
                        && x.Id == rq.UserId
                        && !admins.Contains(x.Id))
            .FirstOrDefaultAsync(cancellationToken);
            
        if (bannedUsers == null)
            throw new ErrorCodeException(ErrorCodes.ACCOUNT_NOTFOUND, $"User with id {rq.UserId} not found");

        bannedUsers.IsBanned = rq.IsBanned;
        await _identityService.BanUser(rq.UserId,  rq.IsBanned);
        await _context.SaveChangesAsync(cancellationToken);
        return bannedUsers.Id;
    }
}
