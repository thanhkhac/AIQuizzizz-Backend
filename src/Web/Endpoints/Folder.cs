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
            .MapPost(CreateFolder, "");
}

    public async Task<Ok<ApiResponse<Guid>>> CreateFolder([FromBody] CreateFolderCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<List<SearchFolderTestDto>>>> SearchFolderTest(
        [FromQuery] string? folderName,
        [FromQuery] string? sharedMode,
        ISender sender)
    {
        var rq = new SearchFolderTest { SharedMode = sharedMode, FolderName = folderName };
        
        var result = await sender.Send(rq);
        
        return result.ToOk();
    }
}
