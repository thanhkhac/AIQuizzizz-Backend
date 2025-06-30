using System.Text.Json;
using CleanArchitectureBase.Application.QuestionSets.Common;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Common.Serializers;

public class QuestionTypeSerializer
{
    public static string SerializeMultipleChoice(List<CreateMultipleChoiceDto> choices)
    {
        var jsonData = choices.Select(c => new QTypeMultipleChoice { Id = Guid.NewGuid(), Text = c.Text!, IsAnswer = c.IsAnswer }).ToList();

        return JsonSerializer.Serialize(jsonData);
    }

    public static string SerializeMatchingPairs(List<CreateMatchingPairDto> pairs)
    {
        var matchingItems = new List<QTypeMatching>();

        foreach (var pair in pairs)
        {
            var leftId = Guid.NewGuid();
            var rightId = Guid.NewGuid();

            //LeftItem với AnswerId là ID của RightItem
            matchingItems.Add(new QTypeMatching { Id = leftId, Text = pair.LeftItem!, AnswerId = rightId.ToString() });

            //RightItem không có AnswerId
            matchingItems.Add(new QTypeMatching { Id = rightId, Text = pair.RightItem!, AnswerId = null });
        }

        return JsonSerializer.Serialize(matchingItems);
    }

    public static string SerializeOrderingItems(List<CreateOrderingItemDto> items)
    {
        var jsonData = items.Select(i => new QTypeOrderingItem { Id = Guid.NewGuid(), Text = i.Text!, CorrectOrder = i.CorrectOrder }).ToList();

        return JsonSerializer.Serialize(jsonData);
    }
}
