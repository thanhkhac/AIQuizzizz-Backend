using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IAiGenerateService
{
    Task<string> SendPromptWithFileAsync(FileStreamData fileData);
}
