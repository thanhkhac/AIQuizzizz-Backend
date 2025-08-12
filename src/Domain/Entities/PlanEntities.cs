using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureBase.Domain.Entities;

public class Plan : BaseAuditableEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required int Price { get; set; }
    public required int Duration { get; set; }
    public required string Unit { get; set; }
    public bool CanLearn { get; set; }
    public bool CanOpenTest { get; set; }
    public bool CanCopyOrImportQuestionSet { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public List<PlanPriceHistory> PriceHistories { get; set; } = new List<PlanPriceHistory>();
}

public class PlanPriceHistory : BaseAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid PlanId { get; set; }

    public required int Price { get; set; }
    public required DateTimeOffset DateStart { get; set; }
    public DateTimeOffset? DateFinish { get; set; }

    public Plan Plan { get; set; } = null!;
}

public class UserSubscription : BaseEntity
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid PlanId { get; set; }
    public required DateTimeOffset DateStart { get; set; }
    public required DateTimeOffset DateFinish { get; set; }
    public int Price { get; set; }
    public int Duration { get; set; }
    public string? Unit { get; set; } 
    public bool IsActive { get; set; }

    public User? User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
}

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? PaymentId { get; set; }
    [MaxLength(255)]
    public string? Code { get; set; }
    public string? Gateway { get; set; }
    public string? TransferType { get; set; }
    public decimal TransferAmount { get; set; }
    public DateTimeOffset? TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? SubAccount { get; set; }
    public decimal? Accumulated { get; set; }
    public string? Content { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? Created { get; set; } = DateTimeOffset.UtcNow;
    public User? User { get; set; }

}
