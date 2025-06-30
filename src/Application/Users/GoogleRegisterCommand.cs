using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Users.Common;
using FluentValidation;

namespace CleanArchitectureBase.Application.Users;

public class GoogleRegisterCommand : IRequest<TokenDto>
{
    public required string AuthorizationCode { get; set; }
}

public class GoogleRegisterCommandValidator : AbstractValidator<GoogleRegisterCommand>
{
    public GoogleRegisterCommandValidator()
    {
        RuleFor(x => x.AuthorizationCode)
            .NotEmpty()
            .WithMessage("AuthorizationCode is required");
    }
}

public class GoogleRegisterCommandHandler : IRequestHandler<GoogleRegisterCommand, TokenDto>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public GoogleRegisterCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<TokenDto> Handle(GoogleRegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.TryGoogleRegisterAsync(request.AuthorizationCode, "https://aiquizizz.com/authentication/google/callback");
        return result;
    }
}
