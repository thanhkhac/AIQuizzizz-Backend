using CleanArchitectureBase.Application.AiGenerate;
using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class AiGenerateEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);
        
        group.MapPost(GenerateQuestionWithAi, "/GenerateQuestion")
            .DisableAntiforgery();
            
        group.MapPost(GenerateDocumentStructure, "GenerateDocumentStructure")
            .DisableAntiforgery();
        
    }
    
    public async Task<Ok<ApiResponse<string>>> GenerateQuestionWithAi(
        [FromForm] IFormFile file,
        ISender sender)
    {
        var rq = new FileStreamData()
        {
            Data = file.OpenReadStream(),
            ContentType = file.ContentType,
            FileName = file.FileName,
        };
        var result = await sender.Send(new GenerateQuestionWithAiCommand
        {
            FileData = rq
        });
        return result.ToOk();
    }
    
    
    public async Task<Ok<ApiResponse<DocumentStructureDto>>> GenerateDocumentStructure(
        [FromForm] IFormFile file,
        ISender sender)
    {
        var rq = new FileStreamData()
        {
            Data = file.OpenReadStream(),
            ContentType = file.ContentType,
            FileName = file.FileName,
        };
        var result = await sender.Send(new GenerateDocumentStructureCommand()
        {
            FileData = rq
        });
        return result.ToOk();
    }
}
