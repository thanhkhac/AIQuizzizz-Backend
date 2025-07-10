namespace CleanArchitectureBase.Application.Questions.Utils;

public static class QuestionShuffleHelper
{
    private static readonly Random _random = new();

    public static List<T> ShuffleWithOrder<T>(List<T> items) where T : class
    {
        return items
            .OrderBy(_ => _random.Next())
            .Select((item, index) =>
            {
                var prop = typeof(T).GetProperty("ShuffleOrder");
                if (prop != null && prop.PropertyType == typeof(short))
                    prop.SetValue(item, (short)index);
                return item;
            })
            .ToList();
    }
}
