using CleanArchitectureBase.Application.Questions.Dtos;

namespace CleanArchitectureBase.Application.Tests.Dto;

public class TestDetailDto
{
    public Guid TestId { get; set; }
    public Guid ClassId { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public int TimeLimit { get; set; }
    public int QuestionCount { get; set; }
    public string? GradeAttemptMethod { get; set; }
    public string? GradeQuestionValue { get; set; }
    public bool IsShowCorrectAnswerInReview { get; set; }
    public bool IsAllowReviewAfterSubmit { get; set; }
    public int MaxAttempt { get; set; }
    public double PassScore { get; set; }
    public List<QuestionResponseDto> Questions { get; set; } = new();
}
