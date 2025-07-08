using CleanArchitectureBase.Application.QuestionSets.Common;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Services;

public static class QuestionTypeMapper
{
    public static List<QTypeMultipleChoice> MapMultipleChoice(List<CreateMultipleChoiceDto> choices)
    {
        return choices.Select(c => new QTypeMultipleChoice
        {
            Id = Guid.NewGuid(),
            Text = c.Text!,
            IsAnswer = c.IsAnswer
        }).ToList();
    }

    public static List<QTypeMatching> MapMatchingPairs(List<CreateMatchingPairDto> pairs)
    {
        var result = new List<QTypeMatching>();
        foreach (var pair in pairs)
        {
            var leftId = Guid.NewGuid();
            var rightId = Guid.NewGuid();

            result.Add(new QTypeMatching { Id = leftId, Text = pair.LeftItem!, AnswerId = rightId.ToString() });
            result.Add(new QTypeMatching { Id = rightId, Text = pair.RightItem!, AnswerId = null });
        }

        return result;
    }

    public static List<QTypeOrderingItem> MapOrderingItems(List<CreateOrderingItemDto> items)
    {
        return items.Select(i => new QTypeOrderingItem
        {
            Id = Guid.NewGuid(),
            Text = i.Text!,
            CorrectOrder = i.CorrectOrder
        }).ToList();
    }
}

