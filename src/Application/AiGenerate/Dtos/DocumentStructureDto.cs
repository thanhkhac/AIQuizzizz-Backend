namespace CleanArchitectureBase.Application.AiGenerate.Dtos;

public class DocumentStructureDto
{
    public string? Title { get; set; }
    public List<DocumentStructureDto> Children { get; set; } = new();
}
