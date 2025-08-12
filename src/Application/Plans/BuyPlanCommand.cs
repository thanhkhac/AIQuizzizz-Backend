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

        var dateFinish = DateTimeOffset.UtcNow;
        if (plan.Unit.ToLower() == "day")
        {
            dateFinish = dateFinish.AddDays(plan.Duration);
        }
        else if (plan.Unit.ToLower() == "month")
        {
            dateFinish = dateFinish.AddMonths(plan.Duration);
        }
        else if (plan.Unit.ToLower() == "year")
        {
            dateFinish = dateFinish.AddYears(plan.Duration);
        }

        var newEntity = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PlanId = plan.Id,
            DateStart = DateTimeOffset.UtcNow,
            DateFinish = dateFinish,
            Price = plan.Price,
            Duration = plan.Duration,
            Unit = plan.Unit,
            IsActive = true,
        };

        _context.UserSubscriptions.Add(newEntity);

        user.Balance -= Convert.ToInt64(plan.Price);

        await _context.SaveChangesAsync(cancellationToken);

        return newEntity.Id;
    }
}
