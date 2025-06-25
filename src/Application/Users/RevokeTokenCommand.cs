using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Users;

[Authorize]
public class RevokeTokenCommand : IRequest
{
    public string? RefreshToken { get; init; }
}

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}


public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;


    public RevokeTokenCommandHandler(IIdentityService identityService, IUser user)
    {
        _identityService = identityService;
        _user = user;
    }

    public async Task Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        await _identityService.RevokeRefreshTokenAsync(request.RefreshToken!, _user.UserId!.Value);
    }
}
