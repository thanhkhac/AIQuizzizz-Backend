using System.Text.Json;
using CleanArchitectureBase.Application.QuestionSets.Common;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Services;

public static class QuestionTypeSerializer
{
    public static string SerializeMultipleChoice(List<CreateMultipleChoiceDto> choices)
    {
        var mapped = QuestionTypeMapper.MapMultipleChoice(choices);
        var shuffled = QuestionShuffleHelper.ShuffleWithOrder(mapped);
        return JsonSerializer.Serialize(shuffled);
    }

    public static string SerializeMatchingPairs(List<CreateMatchingPairDto> pairs)
    {
        var mapped = QuestionTypeMapper.MapMatchingPairs(pairs);
        var shuffled = QuestionShuffleHelper.ShuffleWithOrder(mapped);
        return JsonSerializer.Serialize(shuffled);
    }

    public static string SerializeOrderingItems(List<CreateOrderingItemDto> items)
    {
        var mapped = QuestionTypeMapper.MapOrderingItems(items);
        var shuffled = QuestionShuffleHelper.ShuffleWithOrder(mapped);
        return JsonSerializer.Serialize(shuffled);
    }
}
