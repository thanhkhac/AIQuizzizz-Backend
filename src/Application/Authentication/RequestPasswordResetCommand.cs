using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Users.Common;

namespace CleanArchitectureBase.Application.Users;

public class RequestPasswordResetCommand : IRequest
{
    public required string Email { get; set; }
}

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly IIdentityService _identityService;
    public RequestPasswordResetCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        await _identityService.RequestPasswordResetAsync(new ForgotPasswordDto { Email = request.Email });
    }
}
