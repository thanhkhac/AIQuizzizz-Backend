using System.Text.Json;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.Dto;

public class AttemptDetailDto
{
    public Guid AttemptId { get; set; }
    public string?Name { get; set; }
    public int QuestionCount { get; set; }
    public DateTimeOffset? TimeStart { get; set; }
    public DateTimeOffset? TimeEnd { get; set; }
    public int TimeLimit { get; set; }
    public double TimeRemaining { get; set; }
    public List<QuestionAttemptDetailDto> Questions { get; set; } = new();
}
public class QuestionAttemptDetailDto
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
        public static QuestionAttemptDetailDto FromEntity(Question question, AttemptQuestion? userAnswer)
        {
            return new QuestionAttemptDetailDto
            {
                Id = question.Id,
                Type = question.Type.ToString(),    
                TextFormat = question.TextFormat.ToString(),
                QuestionText = question.QuestionText ?? string.Empty,
                Score = question.Score,
                CorrectMultipleChoiceCount = question.Type == QuestionType.MultipleChoice
                    ? QuestionDataDto.CorrectMultipleChoiceCount(question) : null,
                QuestionData = QuestionDataDto.Deserializer.FromJson(question.Type, question.DataJson, false, false),
                UserAnswerDataDto = userAnswer != null 
                    ? Serializer.DeSerialize(question.Type.ToString(), userAnswer.DataJson)
                    : null
            };
        }
    }
}
