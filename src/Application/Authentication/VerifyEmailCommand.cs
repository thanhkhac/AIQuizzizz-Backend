using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Users.Common;

namespace CleanArchitectureBase.Application.Users;

public class VerifyEmailCommand : IRequest
{
    public required string Email { get; set; }
    public required string VerificationCode { get; set; }
}

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.VerificationCode).NotEmpty();
    }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand>
{
    private readonly IIdentityService _identityService;
    public VerifyEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        await _identityService.VerifyEmailAsync(new EmailVerificationConfirmDto { Email = request.Email, VerificationCode = request.VerificationCode });
    }
}
