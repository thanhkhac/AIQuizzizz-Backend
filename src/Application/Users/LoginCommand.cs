using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Users.Common;

namespace CleanArchitectureBase.Application.Users;

public class LoginCommand : IRequest<TokenDto>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}


public class LoginCommandCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}

public class LoginCommandCommandHandler : IRequestHandler<LoginCommand, TokenDto>
{

    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;


    public LoginCommandCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<TokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.TryLoginAsync(request.Email, request.Password);
        return result;
    }
}
