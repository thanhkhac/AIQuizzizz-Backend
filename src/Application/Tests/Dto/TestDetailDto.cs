using CleanArchitectureBase.Application.Questions.Dtos;

namespace CleanArchitectureBase.Application.Tests.Dto;

public class TestDetailDto
{
    public Guid TestId { get; set; }
    public string?Name { get; set; }
    public int QuestionCount { get; set; }
    public DateTime? TimeStart { get; set; }
    public DateTime? TimeEnd { get; set; }
    public int TimeLimit { get; set; }
    public List<QuestionResponseDto> Questions { get; set; } = new();
}
