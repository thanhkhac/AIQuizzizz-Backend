namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IGoogleAccessTokenProvider
{
    Task<string> GetAccessTokenAsync();
}
