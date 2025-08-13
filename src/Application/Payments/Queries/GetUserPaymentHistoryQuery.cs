using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Payments.Dto;

namespace CleanArchitectureBase.Application.Payments.Queries;

[Authorize]
public class GetUserPaymentHistoryQuery : IRequest<List<PaymentHistoryDto>>
{
    public int PageNumber = 1;
    public int PageSize = 5;
}

public class GetUserPaymentHistoryQueryHandler : IRequestHandler<GetUserPaymentHistoryQuery, List<PaymentHistoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public GetUserPaymentHistoryQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;       
    }
    
    public async Task<List<PaymentHistoryDto>> Handle(GetUserPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        var paymentHistory = await _context.UserSubscriptions
            .Include(x => x.Plan)
            .Where(x => x.UserId.Equals(_user.UserId))
            .Select(x => new PaymentHistoryDto
            {
                Id = x.Id,
                PlanId = x.PlanId,
                PlanName = x.Plan.Name,
                Date = x.DateStart,
                Price = x.Price,
                Status = "Paid"
            })
            .ToListAsync(cancellationToken);
        return paymentHistory;
    }
}
