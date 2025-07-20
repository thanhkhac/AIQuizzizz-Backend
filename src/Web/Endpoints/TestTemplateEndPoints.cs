using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Application.TestTemplates.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class TestTemplate : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(SearchTestTemplate, "/Templates")
            .MapGet(GetTestTemplatePermissions, "/{testTemplateId}/Permissions")
            .MapGet(GetTestTemplateDetail, "/Template/{testTemplateId}")
            .MapPost(CreateTestTemplate, "/Templates");

        app.MapGroup(this).DisableAntiforgery()
            .MapPost(ImportFileTestTemplate, "/Templates/ImportFile");
    }

    public async Task<Ok<ApiResponse<PaginatedList<TestTemplateDto>>>> SearchTestTemplate(
        [FromQuery] string? folderName,
        [FromQuery] string? sharedMode,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchTestTemplateCommand
        {
            TestTemplateName = folderName, SharedMode = sharedMode, PageNumber = pageNumber, PageSize = pageSize,
        };
        
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> CreateTestTemplate([FromBody] CreateTestTemplateCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<TestTemplatePermissionsDto>>> GetTestTemplatePermissions([FromRoute] Guid testTemplateId, ISender sender)
    {
        var query = new GetTestTemplatePermissionsCommand
        {
            TestTemplateId = testTemplateId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<TestTemplateDetailDto>>> GetTestTemplateDetail([FromRoute] Guid testTemplateId, ISender sender)
    {
        var query = new GetTestTemplateDetailCommand
        {
            TestTemplateId = testTemplateId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<ImportedQuestionDto>>> ImportFileTestTemplate(
        [FromForm] IFormFile file,
        ISender sender)
    {
        var rq = new FileStreamData()
        {
            Data = file.OpenReadStream(),
            ContentType = file.ContentType,
            FileName = file.FileName,
        };
        var result = await sender.Send(new ImportFileTestTemplateCommand
        {
            FileData = rq
        });
        return result.ToOk();
    }
}
