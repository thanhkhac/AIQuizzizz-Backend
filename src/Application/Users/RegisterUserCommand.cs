using System.Data;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Users;

public class RegisterUserCommand : IRequest<string>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
{

    private readonly IIdentityService _identityService;

    public RegisterUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.CreateUserAsync(request.Email, request.Password);

        if (!result.Result.Succeeded)
            throw new ErrorCodeException(result.Result.Errors);
        return result.UserId;
    }
}
