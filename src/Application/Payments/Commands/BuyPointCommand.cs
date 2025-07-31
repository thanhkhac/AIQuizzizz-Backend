using System.Text.Json.Serialization;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Utils;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Payments.Commands;

public class BuyPointCommand : IRequest
{
    /// <summary>
    /// Transaction ID on SePay
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Brand name of the bank
    /// </summary>
    public string? Gateway { get; set; }

    /// <summary>
    /// Bank account number
    /// </summary>
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime? TransactionDate { get; set; }

    /// <summary>
    /// Bank account number
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// Sub-bank account (identified account),
    /// </summary>
    public string? SubAccount { get; set; }

    /// <summary>
    /// Payment code (sepay automatically identifies based on the configuration at Company -> General configuration)
    /// Example: PaymentCodePrefix + random 30 character
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Transfer content. Example: Nguyen Khac Thanh chuyen tien
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Transaction type. in is money "in", "out" is money out
    /// </summary>
    public string? TransferType { get; set; }

    /// <summary>
    /// Transaction amount "transferAmount":2277000
    /// </summary>
    public required decimal TransferAmount { get; set; }

    /// <summary>
    /// Full content of sms message
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Reference code of sms message
    /// </summary>
    public string? ReferenceCode { get; set; }

    /// <summary>
    /// Account balance (cumulative)
    /// </summary>
    public decimal? Accumulated { get; set; }

}

public class BuyPointCommandValidator : AbstractValidator<BuyPointCommand>
{
    public BuyPointCommandValidator()
    {
        RuleFor(x => x.TransferType)
            .NotEmpty().WithMessage("TransferType is required.")
            .Must(t => t == "in")
            .WithMessage("Only 'in' transactions are accepted.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}

public class BuyPointCommandHandler : IRequestHandler<BuyPointCommand>
{
    public IApplicationDbContext _context;
    public BuyPointCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }


    public async Task Handle(BuyPointCommand command, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers.FirstOrDefaultAsync(x => x.PaymentCode == command.Code, cancellationToken);
        if (user == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOTFOUND);

        var tranferAmount = (long)Math.Floor(command.TransferAmount);
        var paymentId = command.Id!.ToString();
        user.Balance += tranferAmount;


        var existedTransaction = await _context.Transactions.FirstOrDefaultAsync(x => x.PaymentId == paymentId, cancellationToken);
        if(existedTransaction != null)
            throw new ErrorCodeException(ErrorCodes.PAYMENT_TRANSACTION_EXISTED);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PaymentId = paymentId,
            Code = command.Code,
            Gateway = command.Gateway,
            TransferType = command.TransferType,
            TransferAmount = command.TransferAmount,
            TransactionDate = command.TransactionDate,
            AccountNumber = command.AccountNumber,
            SubAccount = command.SubAccount,
            Accumulated = command.Accumulated,
            Content = command.Content,
            Description = command.Description,
            Created = DateTimeOffset.UtcNow,
        };
        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
