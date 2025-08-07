using CleanArchitectureBase.Application.Questions.Dtos;

namespace CleanArchitectureBase.Application.TestTemplates.Dto;

public class TestTemplateDetailDto
{
    public Guid TestTemplateId { get; set; }
    public string?Name { get; set; }
    public int QuestionCount { get; set; }
    public string? CreateBy { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreateAt { get; set; }
    public List<QuestionResponseDto> Questions { get; set; } = new();
}
