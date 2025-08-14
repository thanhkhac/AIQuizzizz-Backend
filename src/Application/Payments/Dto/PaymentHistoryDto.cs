namespace CleanArchitectureBase.Application.Payments.Dto;

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public Guid? PlanId { get; set; }
    public string? PlanName { get; set; }
    public decimal Price { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset Date { get; set; }
    
}
