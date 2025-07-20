using CleanArchitectureBase.Application.Questions.Dtos;

namespace CleanArchitectureBase.Application.Tests.Dto;

public class TestTemplateDetailDto
{
    public Guid TestTemplateId { get; set; }
    public string?Name { get; set; }
    public int QuestionCount { get; set; }
    public List<QuestionResponseDto> Questions { get; set; } = new();
}
