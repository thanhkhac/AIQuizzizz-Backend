namespace CleanArchitectureBase.Application.Tests.Dto;

public class HistoryTestDto
{
    public Guid AttemptId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public DateTimeOffset? TimeStart { get; set; }
    public DateTimeOffset? TimeSubmit { get; set; }
    public float Score { get; set; }
    public string? Status { get; set; }
    public bool CanReview { get; set; }
}

public class ResultTestOfClassDto
{
    public Guid StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public float Score { get; set; }
    public string? Status { get; set; }
}
