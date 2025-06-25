using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.QuestionSets;

public class GenerateDocumentOutlineCommand : IRequest<List<string>>
{
    public required FileData FileData { get; set; }
}
