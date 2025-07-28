namespace CleanArchitectureBase.Application.Plans.Dto;

public class PlanDetailDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public string? DayDuration { get; set; }
    public bool CanLearn { get; set; }
    public bool CanOpenTest { get; set; }
    public bool CanCopyOrImportQuestionSet { get; set; }
}

public static class DateDurationConverter
{
    public static string ConvertToDurationString(int totalDays)
    {
        if (totalDays < 0)
            return "Số ngày không hợp lệ";
        
        int years = totalDays / 365;
        int remainingDays = totalDays % 365;
        int months = remainingDays / 30;
        int days = remainingDays % 30;
        
        var parts = new List<string>();
        if (years > 0)
            parts.Add($"{years} năm");
        if (months > 0)
            parts.Add($"{months} tháng");
        if (days > 0)
            parts.Add($"{days} ngày");
        
        return parts.Any() ? string.Join(" ", parts) : "0 ngày";
    }
}
