using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Application.Payments.Dto;

namespace CleanArchitectureBase.Application.Payments.Queries;

[Authorize]
public class GetUserPaymentHistoryQuery : IRequest<PaginatedList<PaymentHistoryDto>>
{
    public int PageNumber = 1;
    public int PageSize = 5;
}

public class GetUserPaymentHistoryQueryHandler : IRequestHandler<GetUserPaymentHistoryQuery, PaginatedList<PaymentHistoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    
    public GetUserPaymentHistoryQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;       
    }
    
    public async Task<PaginatedList<PaymentHistoryDto>> Handle(GetUserPaymentHistoryQuery request, CancellationToken cancellationToken)
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
            }).ToListAsync(cancellationToken);

        var topUpHistory = _context.Transactions
            .Where(x => x.UserId.Equals(_user.UserId))
            .Select(x => new PaymentHistoryDto
            {
                Id = x.Id, Date = x.TransactionDate!.Value, Price = x.TransferAmount, Status = "TopUp"
            }).ToList();
        
        paymentHistory.AddRange(topUpHistory);
        
        return PaginatedList<PaymentHistoryDto>.Create(
            paymentHistory.OrderByDescending(x => x.Date).ToList(),
            request.PageNumber,
            request.PageSize
            );
    }
}
