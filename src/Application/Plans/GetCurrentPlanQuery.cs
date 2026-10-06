using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Plans.Dto;

namespace CleanArchitectureBase.Application.Plans;

[Authorize]
public class GetCurrentPlanQuery : IRequest<List<CurrentPlanDto>>
{
}

public class GetCurrentPlanQueryHandler : IRequestHandler<GetCurrentPlanQuery, List<CurrentPlanDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public GetCurrentPlanQueryHandler(
        IApplicationDbContext context,
        IUser user)
    {
        _context = context;
        _user = user;
    }
    
    public async Task<List<CurrentPlanDto>> Handle(GetCurrentPlanQuery request, CancellationToken cancellationToken)
    {
        var userSubscriptions = await _context.UserSubscriptions
            .Where(x => x.UserId == _user.UserId
                        && x.DateFinish >= DateTime.Now)
            .OrderByDescending(x => x.DateFinish)
            .Select(x => new CurrentPlanDto
            {
                PlanId = x.PlanId,
                Duration = x.Duration,
                EndDate = x.DateFinish,
                StartDate = x.DateStart,
                Price = x.Price,
                Unit = x.Unit
            }).ToListAsync(cancellationToken);
        
        return userSubscriptions;
    }
}
