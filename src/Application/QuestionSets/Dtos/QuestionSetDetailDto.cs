using CleanArchitectureBase.Application.Tags.Dto;
using CleanArchitectureBase.Application.Users.Common;

namespace CleanArchitectureBase.Application.QuestionSets.Dtos;

public class QuestionSetDetailDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? VisibilityMode { get; set; }
    public int QuestionCount { get; set; }
    public bool IsDeleted { get; set; }
    public int RatingCount { get; set; }
    public double RatingAverage { get; set; }
    public CreatedByDto? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<TagForListReponseDto>  Tags { get; set; } = new();
}
