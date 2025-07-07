using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tests.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Folder : EndpointGroupBase
{
    public override void Map(WebApplication app){
        app.MapGroup(this)
            .MapPost(CreateFolder, "");
}

    public async Task<Ok<ApiResponse<Guid>>> CreateFolder([FromBody] CreateFolderCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
