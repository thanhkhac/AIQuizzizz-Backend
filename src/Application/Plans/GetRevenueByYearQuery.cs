using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Payments.Dto;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Plans;

public class RevenueDto
{
    public int Month { get; set; }
    public int Revenue { get; set; }
}

[Authorize]
public class GetRevenueByYearQuery : IRequest<List<RevenueDto>>
{
    public int Year { get; set; }
}

public class GetRevenueByYearQueryValidator : AbstractValidator<GetRevenueByYearQuery>
{
    public GetRevenueByYearQueryValidator()
    {
        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000).WithMessage("Year phải lớn hơn 2000");
    }
}

public class GetRevenueByYearQueryHandler : IRequestHandler<GetRevenueByYearQuery, List<RevenueDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    
    public GetRevenueByYearQueryHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }
    
    public async Task<List<RevenueDto>> Handle(GetRevenueByYearQuery request, CancellationToken cancellationToken)
    {
        bool isAdmin = await _identityService
            .IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        if (!isAdmin)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION);

        var revenues = _context.UserSubscriptions
            .Where(x => x.DateStart.Year == request.Year)
            .Select(x => new
            {
                Month = x.DateStart.Month,
                Price = x.Price
            });

        var result = new List<RevenueDto>();
        
        for (int i = 1; i <= 12; i++)
        {
            var revenueByMonth = revenues.Where(x => x.Month == i).Sum(x => x.Price);
            
            result.Add(new RevenueDto
            {
                Month = i,
                Revenue = revenueByMonth
            });
        }
        
        return result;
    }
}
