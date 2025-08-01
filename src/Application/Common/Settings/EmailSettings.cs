namespace CleanArchitectureBase.Application.Common.Settings;

public class EmailSettings
{
    public required string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public required string SmtpUser { get; set; }
    public required string SmtpPass { get; set; }
    public required string From { get; set; }
    public bool EnableSsl { get; set; } = true;
}
