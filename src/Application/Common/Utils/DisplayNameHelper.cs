namespace CleanArchitectureBase.Application.Common.Utils;

public static class DisplayNameHelper
{
    /// <summary>
    /// FullName mặc định của user là email khi đăng ký; khi hiển thị cho người khác thì che bớt phần tên (a***@domain).
    /// Giá trị không phải email được giữ nguyên.
    /// </summary>
    public static string? MaskIfEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var at = value.IndexOf('@');
        if (at <= 0 || at == value.Length - 1 || value.Contains(' ')) return value;
        return value[0] + "***" + value[at..];
    }
}
