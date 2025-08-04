using System.Text.Json;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Questions.Dtos;

/// <summary>
/// 
/// </summary>
public class QuestionResponseDto
{
    public Guid Id { get; set; }
    public Guid QuestionSetId { get; set; } = Guid.Empty;
    public string Type { get; set; } = null!;
    public TextFormat TextFormat { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? ExplainText { get; set; }
    public float Score { get; set; }
    public bool? IsCorrect { get; set; }
    public QuestionDataDto QuestionData { get; set; } = null!;

    /// <summary>
    /// Convert entity to response
    /// </summary>
    public static class Mapper
    {
        public static QuestionResponseDto FromEntity(Question question, bool? isCorrect, bool shuffle = true)
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
                IsCorrect = isCorrect,
                QuestionData = QuestionDataDto.Deserializer.FromJson(question.Type, question.DataJson, shuffle: shuffle)
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
        /// <summary>
        /// Convert JSON data into entities depending on question type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dataJson"></param>
        /// <param name="shuffle"></param>
        /// <returns></returns>
        public static QuestionDataDto FromJson(QuestionType type, string? dataJson, bool shuffle)
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
                    result.Matching = DeserializeMatching(dataJson, shuffle);
                    break;

                case QuestionType.Ordering:
                    result.Ordering = DeserializeOrdering(dataJson, shuffle);
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
            return items?
                .OrderBy(x => x.ShuffleOrder)
                .Select(item => new MultipleChoiceItemDto
                {
                    Id = item.Id,
                    Text = item.Text,
                    IsAnswer = item.IsAnswer
                }).ToList();
        }

        private static MatchingDataDto? DeserializeMatching(string dataJson, bool shuffle = true)
        {
            var items = JsonSerializer.Deserialize<List<QTypeMatching>>(dataJson);
            if (items == null) return null;

            var leftItems = items.Where(x => !string.IsNullOrEmpty(x.AnswerId)).ToList();
            var rightItems = items.Where(x => string.IsNullOrEmpty(x.AnswerId)).ToList();

            IEnumerable<QTypeMatching> orderedRightItems = shuffle
                ? rightItems.OrderBy(x => x.ShuffleOrder)
                : rightItems;

            return new MatchingDataDto
            {
                LeftItems = leftItems
                    .Select(item => new MatchingItemDto
                    {
                        Id = item.Id,
                        Text = item.Text
                    }).ToList(),
                RightItems = orderedRightItems
                    .Select(item => new MatchingItemDto
                    {
                        Id = item.Id,
                        Text = item.Text
                    }).ToList(),
                Matches = leftItems.Select(item => new MatchDto
                {
                    LeftId = Guid.Parse(item.AnswerId!),
                    RightId = item.Id
                }).ToList()
            };
        }

        private static List<OrderingItemDto>? DeserializeOrdering(string dataJson, bool shuffle = true)
        {
            var items = JsonSerializer.Deserialize<List<QTypeOrderingItem>>(dataJson);
            if (items == null) return null;

            IEnumerable<QTypeOrderingItem> orderedItems = shuffle
                ? items.OrderBy(x => x.ShuffleOrder)
                : items;

            return orderedItems
                .Select(item => new OrderingItemDto
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
