using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Users.Common;
using FluentValidation;

namespace CleanArchitectureBase.Application.Users;

public class GoogleLoginCommand : IRequest<TokenDto>
{
    public required string AuthorizationCode { get; set; }
    public required string RedirectUri { get; set; }
}

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.AuthorizationCode)
            .NotEmpty()
            .WithMessage("AuthorizationCode is required");
        RuleFor(x => x.RedirectUri)
            .NotEmpty()
            .WithMessage("RedirectUri is required");
    }
}

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, TokenDto>
{
    private readonly IIdentityService _identityService;

    public GoogleLoginCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
    }

    public async Task<TokenDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.TryGoogleLoginAsync(request.AuthorizationCode, request.RedirectUri);
        return result;
    }
} 
