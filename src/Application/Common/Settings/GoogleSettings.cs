namespace CleanArchitectureBase.Application.Common.Settings;

public class GoogleSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string Issuer { get; set; } = "https://accounts.google.com";
} 
