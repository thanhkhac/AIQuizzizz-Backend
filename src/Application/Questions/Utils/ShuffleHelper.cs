namespace CleanArchitectureBase.Application.Questions.Utils;

public static class ShuffleHelper
{
    private static readonly Random _random = new();

    /// <summary>
    /// Gán ShuffleOrder ngẫu nhiên (hoán vị 0..n-1) nhưng GIỮ NGUYÊN thứ tự mảng (thứ tự của tác giả).
    /// ShuffleOrder chỉ dùng khi giao câu hỏi cho người học (learn/practice/attempt).
    /// </summary>
    public static void AssignShuffleOrder<T>(List<T> items) where T : class
    {
        var prop = typeof(T).GetProperty("ShuffleOrder");
        if (prop == null || prop.PropertyType != typeof(short)) return;

        var permutation = Enumerable.Range(0, items.Count).OrderBy(_ => _random.Next()).ToList();
        for (var i = 0; i < items.Count; i++)
            prop.SetValue(items[i], (short)permutation[i]);
    }

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
