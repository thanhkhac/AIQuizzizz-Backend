using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.TestTemplates;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IFileService
{
    Task<ImportedQuestionDto> GetQuestionFromFile(FileStreamData fileData);
}
