using System.Text.Json;
using CleanArchitectureBase.Application.QuestionSets.Dtos;

namespace CleanArchitectureBase.Application.Questions.Utils;

public static class QuestionTypeSerializer
{
    public static string SerializeMultipleChoice(List<CreateMultipleChoiceDto> choices)
    {
        var mapped = QuestionTypeMapper.MapMultipleChoice(choices);
        ShuffleHelper.AssignShuffleOrder(mapped);
        return JsonSerializer.Serialize(mapped);
    }

    public static string SerializeMatchingPairs(List<CreateMatchingPairDto> pairs)
    {
        var mapped = QuestionTypeMapper.MapMatchingPairs(pairs);
        ShuffleHelper.AssignShuffleOrder(mapped);
        return JsonSerializer.Serialize(mapped);
    }

    public static string SerializeOrderingItems(List<CreateOrderingItemDto> items)
    {
        var mapped = QuestionTypeMapper.MapOrderingItems(items);
        ShuffleHelper.AssignShuffleOrder(mapped);
        return JsonSerializer.Serialize(mapped);
    }
}
