using System.Text.Json;
using CleanArchitectureBase.Application.AiGenerate;
using CleanArchitectureBase.Application.AiGenerate.Dtos;
using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
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
            
        group.MapPost(GenerateDocumentStructure, "/GenerateDocumentStructure")
            .DisableAntiforgery();
        
        group.MapPost(GetCostToGenerateDocument, "/DocumentStructure/GetCostToGenerate")
            .DisableAntiforgery();
    }
    
    public class GenerateQuestionWithAiForm
    {
        public required IFormFile File { get; set; }
        public bool IsGenerateExplain { get; set; }
        public string? Language { get; set; }
        public int QuestionCount { get; set; }
        public List<string> QuestionTypes { get; set; } = new();
        public string? DocumentStructureJson { get; set; }
        public string? SelectedPartJson { get; set; }
        
        public DocumentStructureDto? DocumentStructure
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DocumentStructureJson))
                    return null;
                try
                {
                    return JsonSerializer.Deserialize<DocumentStructureDto>(DocumentStructureJson);
                }
                catch
                {
                    return null; 
                }
            }
            set
            {
                DocumentStructureJson = value == null ? null : JsonSerializer.Serialize(value);
            }
        }

        public DocumentStructureDto? SelectedPart
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectedPartJson))
                    return null;
                try
                {
                    return JsonSerializer.Deserialize<DocumentStructureDto>(SelectedPartJson);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                SelectedPartJson = value == null ? null : JsonSerializer.Serialize(value);
            }
        }
    }
    
    public async Task<Ok<ApiResponse<List<CreateUpdateQuestionDto>>>> GenerateQuestionWithAi(
        [FromForm] GenerateQuestionWithAiForm form,
        ISender sender)
    {
        
        var rq = new FileStreamData
        {
            Data = form.File.OpenReadStream(),
            ContentType = form.File.ContentType,
            FileName = form.File.FileName
        };
        Console.WriteLine(form.DocumentStructure);
        Console.WriteLine(form.SelectedPart);
        var result = await sender.Send(new GenerateQuestionWithAiCommand
        {
            FileData = rq,
            IsGenerateExplain = form.IsGenerateExplain,
            Language = form.Language,
            QuestionCount = form.QuestionCount,
            QuestionTypes = form.QuestionTypes,
            DocumentStructure = form.DocumentStructure,
            SelectedParts = form.SelectedPart
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
    
    
    public async Task<Ok<ApiResponse<AiMinimumCostDto>>> GetCostToGenerateDocument(
        [FromForm] IFormFile file,
        ISender sender)
    {
        var rq = new FileStreamData()
        {
            Data = file.OpenReadStream(),
            ContentType = file.ContentType,
            FileName = file.FileName,
        };
        var result = await sender.Send(new GetCostToGenerateDocumentStructureQuery()
        {
            FileData = rq
        });
        return result.ToOk();
    }
}
