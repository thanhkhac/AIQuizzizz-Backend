using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Plans.Service;

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

    public Task<bool> CanLearn(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;

        return _context.UserSubscriptions
            .IgnoreQueryFilters()
            .Include(us => us.Plan)
            .AnyAsync(us =>
                us.UserId == userId &&
                us.IsActive &&
                us.DateStart <= now &&
                us.DateFinish >= now &&
                us.Plan.CanLearn);
    }

    public Task<bool> CanOpenTest(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;

        return _context.UserSubscriptions
        .IgnoreQueryFilters()
            .Include(us => us.Plan)
            .AnyAsync(us =>
                us.UserId == userId &&
                us.IsActive &&
                us.DateStart <= now &&
                us.DateFinish >= now &&
                us.Plan.CanOpenTest);
    }

    public Task<bool> CanCopyOrImportQuestionSet(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;

        return _context.UserSubscriptions
            .IgnoreQueryFilters()
            .Include(us => us.Plan)
            .AnyAsync(us =>
                us.UserId == userId &&
                us.IsActive &&
                us.DateStart <= now &&
                us.DateFinish >= now &&
                us.Plan.CanCopyOrImportQuestionSet);
    }
}
