namespace CleanArchitectureBase.Application.SystemSettings.Dto;

public class SystemSettingDetailDto
{
    public Guid Id { get; set; }
    public int InputCostPerMillionTokens { get; set; }
    public int OutputCostPerMillionTokens { get; set; }
    public int FixedSystemFee { get; set; }
    public int MaxInputToken { get; set; }
    public int MaxOutputToken { get; set; }
}
