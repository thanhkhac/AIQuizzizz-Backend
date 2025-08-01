using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureBase.Domain.Entities;

public class Plan : BaseAuditableEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required int DayDuration { get; set; }
    public required bool CanLearn { get; set; }
    public required bool CanOpenTest { get; set; }
    public required bool CanCopyOrImportQuestionSet { get; set; }
    public bool IsDeleted { get; set; }
}

public class TokenPackage : BaseAuditableEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required int TokenCount { get; set; }
    public bool IsDeleted { get; set; }
}

public class UserTokenPurchase : BaseAuditableEntity
{
    public required Guid Id { get; set; }
    public required Guid TokenPackageId { get; set; }
    public required Guid UserId { get; set; }

    public TokenPackage TokenPackage { get; set; } = null!;
    public User? User { get; set; } = null!;
}

public class UserSubscription : BaseEntity
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid PlanId { get; set; }
    public required DateTimeOffset DateStart { get; set; }
    public required DateTimeOffset DateFinish { get; set; }
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
    public DateTimeOffset? Created { get; set; } =  DateTimeOffset.UtcNow;
    public User? User { get; set; }
    
}
