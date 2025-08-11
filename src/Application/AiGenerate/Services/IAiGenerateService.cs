using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.AiGenerate.Services;

public interface IAiGenerateService
{
    public Task<string> SendPromptWithFileAsync(
        string fileUri,
        string systemInstruction,
        string prompt,
        double temperature = 0,
        double topP = 1,
        CancellationToken cancellationToken = default);
        
    public Task<string> SendPromptAsync(
        string systemInstruction,
        string prompt,
        double temperature = 0,
        double topP = 1,
        CancellationToken cancellationToken = default);

    public Task<int> CountTokenWithFileAsync(
        string fileUri,
        string systemInstruction,
        string prompt,
        CancellationToken cancellationToken = default);
        
    public Task<int> CountToken(
        string text1,
        string text2,
        CancellationToken cancellationToken = default);

    public Task<(string FileUri, string FileName)> UploadFileAsync(FileStreamData fileData, CancellationToken cancellationToken = default);

    public Task DeleteFileAsync(string fileName);
}
