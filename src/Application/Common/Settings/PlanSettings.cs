namespace CleanArchitectureBase.Application.Common.Settings;

public class PlanSettings
{
    /// <summary>
    /// true: bỏ qua kiểm tra gói (learn / mở test / copy-import) — dùng cho môi trường test khi chưa có cổng thanh toán
    /// </summary>
    public bool FreeAccess { get; set; }
}
