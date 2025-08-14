using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Plans;

public class PlatformOverviewDto
{
    public int Users { get; set; }
    public int Classes { get; set; }
    public int Revenue { get; set; }
    public int UsersHavePlan { get; set; }
}

[Authorize]
public class GetPlatformOverviewQuery : IRequest<PlatformOverviewDto>
{
    
}

public class GetPlatformOverviewQueryHandler : IRequestHandler<GetPlatformOverviewQuery, PlatformOverviewDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public GetPlatformOverviewQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }

    public async Task<PlatformOverviewDto> Handle(GetPlatformOverviewQuery request, CancellationToken cancellationToken)
    {
        bool isAdmin = await _identityService
            .IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        if (!isAdmin)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION);

        var users =  _context.DomainUsers.Count();
        
        var classes = _context.Classes.Count();
        
        var revenue = _context.UserSubscriptions.Sum(x => x.Price);
        
        var usersHavePlan = _context.UserSubscriptions
            .Select(x => x.UserId)
            .Distinct()
            .Count();

        return new PlatformOverviewDto
        {
            Classes = classes, Revenue = revenue, Users = users, UsersHavePlan = usersHavePlan,
        };
    }
}
