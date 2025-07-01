using CleanArchitectureBase.Application.Common.Interfaces;

namespace CleanArchitectureBase.Application.Users;

public class RequestEmailVerificationCommand : IRequest
{
    public required string Email { get; set; }
}

public class RequestEmailVerificationCommandValidator : AbstractValidator<RequestEmailVerificationCommand>
{
    public RequestEmailVerificationCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class RequestEmailVerificationCommandHandler : IRequestHandler<RequestEmailVerificationCommand>
{
    private readonly IIdentityService _identityService;
    public RequestEmailVerificationCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task Handle(RequestEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        await _identityService.RequestEmailVerificationAsync(request.Email);
    }
}
