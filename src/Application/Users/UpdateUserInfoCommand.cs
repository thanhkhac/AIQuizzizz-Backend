using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Users;

[Authorize]
public class UpdateUserInfoCommand : IRequest<Guid>
{
    public required string FullName { get; set; }
}

public class UpdateUserInfoCommandValidator : AbstractValidator<UpdateUserInfoCommand>
{
    public UpdateUserInfoCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName không được trống");
    }
}

public class UpdateUserInfoCommandHandler : IRequestHandler<UpdateUserInfoCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public UpdateUserInfoCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<Guid> Handle(UpdateUserInfoCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers
            .Where(x => x.Id == _user.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        
        user!.FullName = request.FullName;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return user.Id;
    }
}
