namespace CleanArchitectureBase.Application.Plans.Dto;

public class CurrentPlanDto
{
    public Guid PlanId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int Duration { get; set; }
    public string? Unit { get; set; }
    public int Price { get; set; }
}
