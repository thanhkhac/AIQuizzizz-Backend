﻿﻿using System.Text.Json;
using CleanArchitectureBase.Application.MediaFiles.Dtos;
using CleanArchitectureBase.Application.Questions.Utils;
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
    public string? TextFormat { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? ExplainText { get; set; }
    public float Score { get; set; }
    public bool? IsCorrect { get; set; }
    public QuestionDataDto QuestionData { get; set; } = null!;
    public QuestionMediaDto? Media { get; set; }

    /// <summary>
    /// Convert entity to response
    /// </summary>
    public static class Mapper
    {
        public static QuestionResponseDto FromEntity(Question question, bool? isCorrect, bool shuffle = true, bool isShowAnswer = true)
        {
            return new QuestionResponseDto
            {
                Id = question.Id,
                QuestionSetId = question.QuestionSetId ?? Guid.Empty,
                Type = question.Type.ToString(),
                TextFormat = question.TextFormat.ToString(),
                QuestionText = question.QuestionText ?? string.Empty,
                ExplainText = question.ExplainText,
                Score = question.Score,
                IsCorrect = isCorrect,
                QuestionData = QuestionDataDto.Deserializer.FromJson(question.Type, question.DataJson, shuffle: shuffle, isShowAnswer),
                Media = QuestionMediaDto.From(question.MediaId, question.MediaType)
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
        /// <param name="isShowAnswer"></param>
        /// <returns></returns>
        public static QuestionDataDto FromJson(QuestionType type, string? dataJson, bool shuffle, bool isShowAnswer)
        {
            if (string.IsNullOrEmpty(dataJson))
                return new QuestionDataDto();

            var result = new QuestionDataDto();

            switch (type)
            {
                case QuestionType.MultipleChoice:
                    result.MultipleChoice = DeserializeMultipleChoice(dataJson, isShowAnswer, shuffle);
                    break;

                case QuestionType.Matching:
                    result.Matching = DeserializeMatching(dataJson, shuffle, isShowAnswer);
                    break;

                case QuestionType.Ordering:
                    result.Ordering = DeserializeOrdering(dataJson, shuffle ,isShowAnswer);
                    break;

                case QuestionType.ShortText:
                    result.ShortText = DeserializeShortText(dataJson, isShowAnswer);
                    break;
            }

            return result;
        }

        public static List<MultipleChoiceItemDto>? DeserializeMultipleChoice(string dataJson, bool isShowAnswer = true, bool shuffle = true)
        {
            var items = JsonSerializer.Deserialize<List<QTypeMultipleChoice>>(dataJson);
            if (items == null) return null;

            // shuffle = true: thứ tự giao cho người học (ShuffleOrder); false: thứ tự tác giả (Position)
            return QuestionOrderHelper.Order(items, shuffle, x => x.Position, x => x.ShuffleOrder)
                .Select(item => new MultipleChoiceItemDto
                {
                    Id = item.Id,
                    Text = item.Text,
                    IsAnswer = isShowAnswer ? item.IsAnswer : null
                }).ToList();
        }

        public static MatchingDataDto? DeserializeMatching(string dataJson, bool shuffle = true, bool isShowAnswer = true)
        {
            var items = JsonSerializer.Deserialize<List<QTypeMatching>>(dataJson);
            if (items == null) return null;

            var leftItems = QuestionOrderHelper.AuthorOrder(items.Where(x => !string.IsNullOrEmpty(x.AnswerId)), x => x.Position);
            var rightItems = items.Where(x => string.IsNullOrEmpty(x.AnswerId)).ToList();

            IEnumerable<QTypeMatching> orderedRightItems =
                QuestionOrderHelper.Order(rightItems, shuffle, x => x.Position, x => x.ShuffleOrder);

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
                Matches = isShowAnswer ? leftItems.Select(item => new MatchDto
                {
                    LeftId = item.Id,
                    RightId = Guid.Parse(item.AnswerId!)
                }).ToList() : null
            };
        }

        public static List<OrderingItemDto>? DeserializeOrdering(string dataJson, bool shuffle = true, bool isShowAnswer = true)
        {
            var items = JsonSerializer.Deserialize<List<QTypeOrderingItem>>(dataJson);
            if (items == null) return null;

            IEnumerable<QTypeOrderingItem> orderedItems =
                QuestionOrderHelper.Order(items, shuffle, x => x.Position, x => x.ShuffleOrder);

            return orderedItems
                .Select(item => new OrderingItemDto
                {
                    Id = item.Id,
                    Text = item.Text,
                    CorrectOrder = isShowAnswer ? item.CorrectOrder : null
                }).ToList();
        }


        private static string? DeserializeShortText(string dataJson,  bool isShowAnswer = true)
        {
            var answer = JsonSerializer.Deserialize<QTypeShortAnswer>(dataJson);
            return isShowAnswer ? answer?.Answer : null;
        }
    }
              public static int? CorrectMultipleChoiceCount(Question question)
      {
          var multipleChoice = QuestionDataDto.Deserializer.DeserializeMultipleChoice(question.DataJson!);
          
          return multipleChoice?.Count(x => x.IsAnswer!.Value);
      }
}

public class MultipleChoiceItemDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public bool? IsAnswer { get; set; }
}

public class MatchingDataDto
{
    public List<MatchingItemDto> LeftItems { get; set; } = new();
    public List<MatchingItemDto> RightItems { get; set; } = new();
    public List<MatchDto>? Matches { get; set; } = new();
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
    public int? CorrectOrder { get; set; }
}
