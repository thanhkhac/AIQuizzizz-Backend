namespace CleanArchitectureBase.Application.Tests.Dto;

public class TestScheduleResponse
{
    public DateTimeOffset Date { get; set; }
    public List<TestScheduleDto> TestSchedules { get; set; } = new();
}

public class TestScheduleDto
{
    public DateTimeOffset Date;
    public Guid TestId { get; set; }
    public Guid ClassId { get; set; }
    public string? TestName { get; set; }
    public string? ClassName { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset TimeStart { get; set; }
    public DateTimeOffset TimeFinish { get; set; }
}
