using CleanArchitectureBase.Application.Common.Extensions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Users.Common;

namespace CleanArchitectureBase.Application.Users;

public class ResetPasswordCommand : IRequest
{
    public required string Email { get; set; }
    public required string ResetCode { get; set; }
    public required string NewPassword { get; set; }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.ResetCode).NotEmpty();
        RuleFor(x => x.NewPassword).MustBeValidPassword();
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IIdentityService _identityService;
    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        await _identityService.ResetPasswordAsync(new ResetPasswordDto { Email = request.Email, ResetCode = request.ResetCode, NewPassword = request.NewPassword });
    }
}
