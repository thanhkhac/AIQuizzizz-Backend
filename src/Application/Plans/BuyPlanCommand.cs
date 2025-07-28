using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Plans;

[Authorize]
public class BuyPlanCommand : IRequest<Guid>
{
    public required Guid PlanId { get; set; }
}

public class BuyPlanCommandValidator : AbstractValidator<BuyPlanCommand>
{
    public BuyPlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("PlanId không được để trống");
    }
}

public class BuyPlanCommandHandler : IRequestHandler<BuyPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public BuyPlanCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;       
    }
    
    public async Task<Guid> Handle(BuyPlanCommand rq, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.Where(x => x.Id.Equals(rq.PlanId) && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (plan == null)
            throw new ErrorCodeException(ErrorCodes.PLAN_NOT_FOUND, "Không tìm thấy plan");
        
        var user = await _context.DomainUsers
            .Include(x => x.UserSubscriptions)
            .Where(x => x.Id.Equals(_user.UserId) && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);

        if (user!.Balance < plan.Price)
            throw new ErrorCodeException(ErrorCodes.INSUFFICIENT_BALANCE, "Số dư không đủ");
        
        var userSubscription = user.UserSubscriptions.FirstOrDefault(x => x.PlanId == plan.Id)
            ?? _context.UserSubscriptions.Add(new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PlanId = plan.Id,
                DateStart = DateTimeOffset.Now,
                DateFinish = DateTimeOffset.Now.AddDays(plan.DayDuration),
                IsActive = true
            }).Entity;
        
            userSubscription.DateStart = DateTime.UtcNow;
            userSubscription.DateFinish = DateTime.UtcNow.AddDays(plan.DayDuration);
            
            user.Balance -= Convert.ToInt64(plan.Price);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return userSubscription.Id;
    }
}
