using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.FolderTest;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Folder : EndpointGroupBase
{
    public override void Map(WebApplication app){
        app.MapGroup(this)
            .MapGet(SearchFolderTest, "")
            .MapGet(SearchTestTemplateInFolder, "/{FolderId}/TestTemplates")
            .MapPost(CreateFolder, "");
}

    public async Task<Ok<ApiResponse<Guid>>> CreateFolder([FromBody] CreateFolderCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<SearchFolderTestDto>>>> SearchFolderTest(
        [FromQuery] string? folderName,
        [FromQuery] string? sharedMode,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchFolderTestQuery
        {
            SharedMode = sharedMode,
            FolderName = folderName,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        
        var result = await sender.Send(rq);
        
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<TestTemplateDto>>>> SearchTestTemplateInFolder(
        [FromRoute] Guid FolderId,
        [FromQuery] string? TestTemplateName,
        [FromQuery] string? sharedMode,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5)
    {
        var rq = new SearchTestTemplateInFolderQuery
        {
            FolderId = FolderId,
            TestTemplateName = TestTemplateName,
            SharedMode = sharedMode,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        
        var result = await sender.Send(rq);
        
        return result.ToOk();
    }
}
