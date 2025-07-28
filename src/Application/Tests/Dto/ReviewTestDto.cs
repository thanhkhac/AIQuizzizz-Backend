namespace CleanArchitectureBase.Application.Tests.Dto;

public class ReviewTestDto
{
    public Guid AttemptId { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset TimeStart { get; set; }
    public DateTimeOffset TimeEnd { get; set; }
    public double Score { get; set; }
    public string? Status { get; set; }
}
