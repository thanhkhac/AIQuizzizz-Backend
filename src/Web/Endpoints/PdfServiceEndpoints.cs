using CleanArchitectureBase.Application.AiGenerate;
using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Pdf;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class PdfServiceEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
        
        group.MapPost(GetPdfStructure, "/GetPdfStructure")
            .DisableAntiforgery();
    }
    
    public async Task<Ok<ApiResponse<DocumentStructureDto>>> GetPdfStructure(
        [FromForm] IFormFile file,
        ISender sender)
    {
        var rq = new FileStreamData()
        {
            Data = file.OpenReadStream(),
            ContentType = file.ContentType,
            FileName = file.FileName,
        };
        var result = await sender.Send(new GetPdfStructureQuery()
        {
            FileData = rq
        });
        return result.ToOk();
    }
}
