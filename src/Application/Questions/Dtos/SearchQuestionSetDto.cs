namespace CleanArchitectureBase.Application.QuestionSets.Dtos;

public class SearchQuestionSetDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int NumberOfQuestions { get; set; }
    public int RatingCount { get; set; }
    public double RatingAverage { get; set; }
    public string? CreateBy { get; set; }
}
