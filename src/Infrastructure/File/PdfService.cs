using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.Common.Interfaces;

namespace CleanArchitectureBase.Infrastructure.File;

public class PdfService : IPdfService
{

    public List<DocumentStructureDto> ExtractStructure(Stream pdfStream)
    {
        throw new NotImplementedException();
    }
}
