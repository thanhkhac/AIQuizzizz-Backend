namespace CleanArchitectureBase.Application.Classes.Dto;

public class TestScheduleResponse
{
    public DateTimeOffset Date { get; set; }
    public List<TestScheduleDto> TestSchedules { get; set; } = new();
}

public class TestScheduleDto
{
    public DateTimeOffset Date;
    public Guid TestId { get; set; }
    public string? TestName { get; set; }
}
