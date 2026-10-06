namespace CleanArchitectureBase.Application.Questions.Utils;

public static class QuestionOrderHelper
{
    /// <summary>
    /// Thứ tự của tác giả: theo Position, hòa thì theo vị trí trong mảng JSON (ổn định).
    /// Dữ liệu cũ (Position = 0 hết) tự động rơi về thứ tự mảng.
    /// </summary>
    public static List<T> AuthorOrder<T>(IEnumerable<T> items, Func<T, int> position)
    {
        return items
            .Select((item, index) => (item, index))
            .OrderBy(x => position(x.item))
            .ThenBy(x => x.index)
            .Select(x => x.item)
            .ToList();
    }

    /// <summary>Thứ tự giao cho người học: theo ShuffleOrder, hòa thì theo vị trí mảng.</summary>
    public static List<T> DeliveryOrder<T>(IEnumerable<T> items, Func<T, int> shuffleOrder)
    {
        return items
            .Select((item, index) => (item, index))
            .OrderBy(x => shuffleOrder(x.item))
            .ThenBy(x => x.index)
            .Select(x => x.item)
            .ToList();
    }

    public static List<T> Order<T>(IEnumerable<T> items, bool shuffle, Func<T, int> position, Func<T, int> shuffleOrder)
        => shuffle ? DeliveryOrder(items, shuffleOrder) : AuthorOrder(items, position);
}
