namespace CleanArchitectureBase.Application.Tags.Dto;

public class TagForListReponseDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; } = "";
    public int QuestionSetCount { get; set; }
}
