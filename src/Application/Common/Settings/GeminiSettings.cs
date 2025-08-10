namespace CleanArchitectureBase.Application.Common.Settings;

public class GeminiSettings
{
    public string? ApiKey { get; set; }
    public string? UploadFileUri { get; set; }
    public string? GenerateUri { get; set; }
    public string? ProjectId { get; set; }
}

public static class SystemSettings
{
    public static int InputCostPerMillionTokens = 2_622;
    public static int OutputCostPerMillionTokens = 10_489;
    public static int FixedSystemFee = 1_000;
    public static int MaxInputToken = 1_048_570;
    public static int MaxOutputToken = 65_530;
}
