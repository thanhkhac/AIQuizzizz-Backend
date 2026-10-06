using CleanArchitectureBase.Application.Common.Extensions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using FluentValidation;
using MediatR;

namespace CleanArchitectureBase.Application.Users;

[Authorize]
public class ChangePasswordCommand : IRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required");
        RuleFor(x => x.NewPassword).MustBeValidPassword();
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public ChangePasswordCommandHandler(IIdentityService identityService, IUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        await _identityService.ChangePasswordAsync(_currentUser.UserId!.Value, request.CurrentPassword, request.NewPassword);
    }
}
