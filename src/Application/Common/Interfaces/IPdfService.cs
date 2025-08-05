using CleanArchitectureBase.Application.AiGenerate.Dtos;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IPdfService
{
    public List<DocumentStructureDto> ExtractStructure(Stream pdfStream);
}
