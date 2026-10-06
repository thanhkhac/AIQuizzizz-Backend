using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Questions.Utils;

public static class QuestionTypeMapper
{
    public static List<QTypeMultipleChoice> MapMultipleChoice(List<CreateMultipleChoiceDto> choices)
    {
        return choices.Select((c, index) => new QTypeMultipleChoice
        {
            Id = Guid.NewGuid(),
            Text = c.Text!,
            IsAnswer = c.IsAnswer,
            Position = index
        }).ToList();
    }

    public static List<QTypeMatching> MapMatchingPairs(List<CreateMatchingPairDto> pairs)
    {
        var result = new List<QTypeMatching>();
        for (var index = 0; index < pairs.Count; index++)
        {
            var pair = pairs[index];
            var leftId = Guid.NewGuid();
            var rightId = Guid.NewGuid();

            result.Add(new QTypeMatching { Id = leftId, Text = pair.LeftItem!, AnswerId = rightId.ToString(), Position = index });
            result.Add(new QTypeMatching { Id = rightId, Text = pair.RightItem!, AnswerId = null, Position = index });
        }

        return result;
    }

    public static List<QTypeOrderingItem> MapOrderingItems(List<CreateOrderingItemDto> items)
    {
        return items.Select((i, index) => new QTypeOrderingItem
        {
            Id = Guid.NewGuid(),
            Text = i.Text!,
            CorrectOrder = i.CorrectOrder,
            Position = index
        }).ToList();
    }
}

