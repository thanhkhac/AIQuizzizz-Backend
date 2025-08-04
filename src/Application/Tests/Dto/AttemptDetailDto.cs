using System.Text.Json;
using CleanArchitectureBase.Application.Questions.Dtos;
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
    public TextFormat TextFormat { get; set; }
    public string QuestionText { get; set; } = null!;
    public float Score { get; set; }
    public QuestionAttemptDataDto QuestionData { get; set; } = null!;
    public UserAnswerDataDto? UserAnswerDataDto { get; set; }
    
    public static class Mapper
    {
        public static QuestionAttemptDetailDto FromEntity(Question question, AttemptQuestion? userAnswer)
        {
            return new QuestionAttemptDetailDto
            {
                Id = question.Id,
                Type = question.Type.ToString(),
                TextFormat = question.TextFormat,
                QuestionText = question.QuestionText ?? string.Empty,
                Score = question.Score,
                QuestionData = QuestionAttemptDataDto.Deserializer.FromJson(question.Type, question.DataJson),
                UserAnswerDataDto = userAnswer != null 
                    ? Serializer.DeSerialize(question.Type.ToString(), userAnswer.DataJson)
                    : null
            };
        }
    }
}

public class QuestionAttemptDataDto
{
    public List<MultipleChoiceAttemptItemDto>? MultipleChoice { get; set; }
    public MatchingAttemptDataDto? Matching { get; set; }
    public List<OrderingAttemptItemDto>? Ordering { get; set; }
    public string? ShortText { get; set; }

    public static class Deserializer
    {
        public static QuestionAttemptDataDto FromJson(QuestionType type, string? dataJson)
        {
            if (string.IsNullOrEmpty(dataJson))
                return new QuestionAttemptDataDto();

            var result = new QuestionAttemptDataDto();

            switch (type)
            {
                case QuestionType.MultipleChoice:
                    result.MultipleChoice = DeserializeMultipleChoiceForAttempt(dataJson);
                    break;

                case QuestionType.Matching:
                    result.Matching = DeserializeMatchingForAttempt(dataJson);
                    break;

                case QuestionType.Ordering:
                    result.Ordering = DeserializeOrderingForAttempt(dataJson);
                    break;
            }

            return result;
        }
        
        private static List<MultipleChoiceAttemptItemDto>? DeserializeMultipleChoiceForAttempt(string dataJson)
        {
            var items = JsonSerializer.Deserialize<List<QTypeMultipleChoice>>(dataJson);
            return items?
                .OrderBy(x => x.ShuffleOrder)
                .Select(item => new MultipleChoiceAttemptItemDto
            {
                Id = item.Id,
                Text = item.Text,
            }).ToList();
        }
        
        private static MatchingAttemptDataDto? DeserializeMatchingForAttempt(string dataJson)
        {
            var items = JsonSerializer.Deserialize<List<QTypeMatching>>(dataJson);
            if (items == null) return null;

            var leftItems = items.Where(x => string.IsNullOrEmpty(x.AnswerId)).ToList();
            var rightItems = items.Where(x => !string.IsNullOrEmpty(x.AnswerId)).ToList();

            return new MatchingAttemptDataDto
            {
                LeftItems = leftItems
                    .OrderBy(x => x.ShuffleOrder)
                    .Select(item => new MatchingItemDto
                {
                    Id = item.Id,
                    Text = item.Text
                }).ToList(),
                RightItems = rightItems
                    .OrderBy(x => x.ShuffleOrder)
                    .Select(item => new MatchingItemDto
                {
                    Id = item.Id,
                    Text = item.Text
                }).ToList(),
            };
        }
        
        private static List<OrderingAttemptItemDto>? DeserializeOrderingForAttempt(string dataJson)
        {
            var items = JsonSerializer.Deserialize<List<QTypeOrderingItem>>(dataJson);
            return items?
                .OrderBy(x => x.ShuffleOrder)
                .Select(item => new OrderingAttemptItemDto
            {
                Id = item.Id,
                Text = item.Text,
            }).ToList();
        }
    }
}

public class MultipleChoiceAttemptItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
}

public class MatchingAttemptDataDto
{
    public List<MatchingItemDto> LeftItems { get; set; } = new();
    public List<MatchingItemDto> RightItems { get; set; } = new();
}

public class OrderingAttemptItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
}
