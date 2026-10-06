using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Settings;
using Microsoft.Extensions.Options;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Plans.Service;

public interface IPlanService
{
    public Task<bool> CanLearn(Guid userId);
    public Task<bool> CanOpenTest(Guid userId);
    public Task<bool> CanCopyOrImportQuestionSet(Guid userId);
    public Task<bool> CanUploadImage(Guid userId);
    public Task<bool> CanUploadVideo(Guid userId);
}

public class PlanService : IPlanService
{
    private readonly IApplicationDbContext _context;
    private readonly bool _freeAccess;

    public PlanService(IApplicationDbContext context, IOptions<PlanSettings> planSettings)
    {
        _context = context;
        _freeAccess = planSettings.Value.FreeAccess;
    }

    public Task<bool> CanLearn(Guid userId)
    {
        if (_freeAccess) return Task.FromResult(true);

        var now = DateTimeOffset.UtcNow;

        return _context.UserSubscriptions
            .IgnoreQueryFilters()
            .Include(us => us.Plan)
            .AnyAsync(us =>
                us.UserId == userId &&
                us.DateStart <= now &&
                us.DateFinish >= now &&
                us.Plan.CanLearn);
    }

    public Task<bool> CanOpenTest(Guid userId)
    {
        if (_freeAccess) return Task.FromResult(true);

        var now = DateTimeOffset.UtcNow;

        return _context.UserSubscriptions
        .IgnoreQueryFilters()
            .Include(us => us.Plan)
            .AnyAsync(us =>
                us.UserId == userId &&
                us.DateStart <= now &&
                us.DateFinish >= now &&
                us.Plan.CanOpenTest);
    }

    public Task<bool> CanCopyOrImportQuestionSet(Guid userId)
    {
        if (_freeAccess) return Task.FromResult(true);

        var now = DateTimeOffset.UtcNow;

        return _context.UserSubscriptions
            .IgnoreQueryFilters()
            .Include(us => us.Plan)
            .AnyAsync(us =>
                us.UserId == userId &&
                us.DateStart <= now &&
                us.DateFinish >= now &&
                us.Plan.CanCopyOrImportQuestionSet);
    }

    // Quyền upload media luôn theo gói (không áp dụng FreeAccess) để admin cấu hình được theo từng gói
    public Task<bool> CanUploadImage(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        return _context.UserSubscriptions
            .IgnoreQueryFilters()
            .AnyAsync(us => us.UserId == userId && us.DateStart <= now && us.DateFinish >= now && us.Plan.CanUploadImage);
    }

    public Task<bool> CanUploadVideo(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        return _context.UserSubscriptions
            .IgnoreQueryFilters()
            .AnyAsync(us => us.UserId == userId && us.DateStart <= now && us.DateFinish >= now && us.Plan.CanUploadVideo);
    }
}
