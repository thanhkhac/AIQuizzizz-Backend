using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.Dto;

public class ReviewTestDto
{
    public Guid AttemptId { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset TimeStart { get; set; }
    public DateTimeOffset TimeEnd { get; set; }
    public double Score { get; set; }
    public string? Status { get; set; }
    public List<ReviewQuestionDto> Questions { get; set; } = new();
}

public class ReviewQuestionDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string? TextFormat { get; set; }
    public string QuestionText { get; set; } = null!;
    public float Score { get; set; }
    public int? CorrectMultipleChoiceCount { get; set; }
    public QuestionDataDto QuestionData { get; set; } = null!;
    public UserAnswerDataDto? UserAnswerDataDto { get; set; }
    
    public static class Mapper
    {
        public static ReviewQuestionDto FromEntity(Question question, AttemptQuestion? userAnswer, bool isShowCorrectAnswer)
        {
            return new ReviewQuestionDto
            {
                Id = question.Id,
                Type = question.Type.ToString(),    
                TextFormat = question.TextFormat.ToString(),
                QuestionText = question.QuestionText ?? string.Empty,
                Score = userAnswer!= null ? userAnswer.Score : 0,
                CorrectMultipleChoiceCount = question.Type == QuestionType.MultipleChoice
                    ? QuestionDataDto.CorrectMultipleChoiceCount(question) : null,
                QuestionData = QuestionDataDto.Deserializer.FromJson(question.Type, question.DataJson, false, isShowCorrectAnswer),
                UserAnswerDataDto = userAnswer != null 
                    ? Serializer.DeSerialize(question.Type.ToString(), userAnswer.DataJson)
                    : null          
            };
        }
    }
}
