using CleanArchitectureBase.Application.AiGenerate.Dtos;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface IPdfService
{
    public Task<DocumentStructureDto> ExtractStructure(Stream pdfStream);
    public void TrValidatePdf(Stream pdfStream, int maxPageCount);
}
