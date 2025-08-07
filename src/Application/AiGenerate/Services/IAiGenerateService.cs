using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.AiGenerate.Services;

public interface IAiGenerateService
{
    public Task<string> SendPromptWithFileAsync(
        FileStreamData fileData,
        string systemInstruction,
        string prompt,
        CancellationToken cancellationToken = default);

    public Task<int> CountTokenAsync(
        FileStreamData fileData,
        string systemInstruction,
        string content,
        CancellationToken cancellationToken = default);
}
