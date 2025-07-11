using System.Text.Json;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Questions.Dtos;

public class QuestionResponseDto
{
    public Guid Id { get; set; }
    public Guid QuestionSetId { get; set; }
    public string Type { get; set; } = null!;
    public TextFormat TextFormat { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? ExplainText { get; set; }
    public float Score { get; set; }
    public bool Completed { get; set; }
    public QuestionDataDto QuestionData { get; set; } = null!;

    public static class Mapper
    {
        public static QuestionResponseDto FromEntity(Question question, bool completed)
        {
            return new QuestionResponseDto
            {
                Id = question.Id,
                QuestionSetId = question.QuestionSetId ?? Guid.Empty,
                Type = question.Type.ToString(),
                TextFormat = question.TextFormat,
                QuestionText = question.QuestionText ?? string.Empty,
                ExplainText = question.ExplainText,
                Score = question.Score,
                Completed = completed,
                QuestionData = QuestionDataDto.Deserializer.FromJson(question.Type, question.DataJson)
            };
        }
    }
}

public class QuestionDataDto
{
    public List<MultipleChoiceItemDto>? MultipleChoice { get; set; }
    public MatchingDataDto? Matching { get; set; }
    public List<OrderingItemDto>? Ordering { get; set; }
    public string? ShortText { get; set; }


    public static class Deserializer
    {
        public static QuestionDataDto FromJson(QuestionType type, string? dataJson)
        {
            if (string.IsNullOrEmpty(dataJson))
                return new QuestionDataDto();

            var result = new QuestionDataDto();

            switch (type)
            {
                case QuestionType.MultipleChoice:
                    result.MultipleChoice = DeserializeMultipleChoice(dataJson);
                    break;

                case QuestionType.Matching:
                    result.Matching = DeserializeMatching(dataJson);
                    break;

                case QuestionType.Ordering:
                    result.Ordering = DeserializeOrdering(dataJson);
                    break;

                case QuestionType.ShortText:
                    result.ShortText = DeserializeShortText(dataJson);
                    break;
            }

            return result;
        }

        private static List<MultipleChoiceItemDto>? DeserializeMultipleChoice(string dataJson)
        {
            var items = JsonSerializer.Deserialize<List<QTypeMultipleChoice>>(dataJson);
            return items?.Select(item => new MultipleChoiceItemDto
            {
                Id = item.Id,
                Text = item.Text,
                IsAnswer = item.IsAnswer
            }).ToList();
        }

        private static MatchingDataDto? DeserializeMatching(string dataJson)
        {
            var items = JsonSerializer.Deserialize<List<QTypeMatching>>(dataJson);
            if (items == null) return null;

            var leftItems = items.Where(x => string.IsNullOrEmpty(x.AnswerId)).ToList();
            var rightItems = items.Where(x => !string.IsNullOrEmpty(x.AnswerId)).ToList();

            return new MatchingDataDto
            {
                LeftItems = leftItems.Select(item => new MatchingItemDto
                {
                    Id = item.Id,
                    Text = item.Text
                }).ToList(),
                RightItems = rightItems.Select(item => new MatchingItemDto
                {
                    Id = item.Id,
                    Text = item.Text
                }).ToList(),
                Matches = rightItems.Select(item => new MatchDto
                {
                    LeftId = Guid.Parse(item.AnswerId!),
                    RightId = item.Id
                }).ToList()
            };
        }

        private static List<OrderingItemDto>? DeserializeOrdering(string dataJson)
        {
            var items = JsonSerializer.Deserialize<List<QTypeOrderingItem>>(dataJson);
            return items?.Select(item => new OrderingItemDto
            {
                Id = item.Id,
                Text = item.Text,
                CorrectOrder = item.CorrectOrder
            }).ToList();
        }

        private static string? DeserializeShortText(string dataJson)
        {
            var answer = JsonSerializer.Deserialize<QTypeShortAnswer>(dataJson);
            return answer?.Answer;
        }
    }
}

public class MultipleChoiceItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsAnswer { get; set; }
}

public class MatchingDataDto
{
    public List<MatchingItemDto> LeftItems { get; set; } = new();
    public List<MatchingItemDto> RightItems { get; set; } = new();
    public List<MatchDto> Matches { get; set; } = new();
}

public class MatchingItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
}

public class MatchDto
{
    public Guid LeftId { get; set; }
    public Guid RightId { get; set; }
}

public class OrderingItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public int CorrectOrder { get; set; }
}
