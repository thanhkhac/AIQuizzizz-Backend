using CleanArchitectureBase.Application.Tags.Dto;

namespace CleanArchitectureBase.Application.Questions.Dtos;

public class QuestionSetForListResponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int NumberOfQuestions { get; set; }
    public int RatingCount { get; set; }
    public double RatingAverage { get; set; }
    public string? CreateBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public List<TagForListReponseDto>  Tags { get; set; } = new();
    public DateTimeOffset? LastAccessByMe { get; set; }
}
