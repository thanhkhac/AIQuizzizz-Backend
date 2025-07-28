using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Plans;

[Authorize]
public class DeletePlanCommand : IRequest<Guid>
{
    public required Guid PlanId { get; init; }
}

public class DeletePlanCommandValidator : AbstractValidator<DeletePlanCommand>
{
    public DeletePlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("PlanId không được để trống");
    }
}

public class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    
    public DeletePlanCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;       
    }
    
    public async Task<Guid> Handle(DeletePlanCommand rq, CancellationToken cancellationToken)
    {
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        if (!isAdmin)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION, "Không có quyền xóa plan");
        
        var plan = await _context.Plans
            .Where(x => x.Id.Equals(rq.PlanId) && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (plan == null)
            throw new ErrorCodeException(ErrorCodes.PLAN_NOT_FOUND, "Không tìm thấy plan");
        
        plan.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return plan.Id;
    }
}
