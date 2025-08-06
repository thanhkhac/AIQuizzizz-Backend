using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IAiGenerateService
{
    public Task<string> SendPromptWithFileAsync(
        FileStreamData fileData,
        string prompt,
        CancellationToken cancellationToken = default);
}
