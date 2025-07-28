using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Plans;

public interface IPlanService
{
    public Task<bool> CanLearn(Guid userId);
    public Task<bool> CanOpenTest(Guid userId);
    public Task<bool> CanCopyOrImportQuestionSet(Guid userId);
}

public class PlanService : IPlanService
{
    private readonly IApplicationDbContext _context;

    public PlanService(IApplicationDbContext context)
    {
        _context = context;
    }

    private async Task<bool> HasActiveSubscriptionWithFeature(Guid userId, Func<Plan, bool> featureSelector)
    {
        var activeSubscription = await _context.UserSubscriptions
            .Include(us => us.Plan)
            .Where(us => us.UserId == userId
                         && us.IsActive
                         && us.DateStart <= DateTimeOffset.UtcNow
                         && us.DateFinish >= DateTimeOffset.UtcNow)
            .OrderByDescending(us => us.DateFinish) 
            .FirstOrDefaultAsync();

        if (activeSubscription == null)
            return false;

        return featureSelector(activeSubscription.Plan);
    }

    public Task<bool> CanLearn(Guid userId)
    {
        return HasActiveSubscriptionWithFeature(userId, plan => plan.CanLearn);
    }

    public Task<bool> CanOpenTest(Guid userId)
    {
        return HasActiveSubscriptionWithFeature(userId, plan => plan.CanOpenTest);
    }

    public Task<bool> CanCopyOrImportQuestionSet(Guid userId)
    {
        return HasActiveSubscriptionWithFeature(userId, plan => plan.CanCopyOrImportQuestionSet);
    }
}
