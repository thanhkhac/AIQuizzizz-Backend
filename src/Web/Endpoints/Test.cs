using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Application.Tests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(SearchTestTemplate, "/Templates")
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
        var rq = new SearchTestTemplate
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

    public async Task<Ok<ApiResponse<ImportedQuestionDto>>> ImportFileTestTemplate(
        [FromForm] IFormFile fileData,
        ISender sender)
    {
        var rq = new ImportFileTestTemplateCommand
        {
            FileData = fileData
        };
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
