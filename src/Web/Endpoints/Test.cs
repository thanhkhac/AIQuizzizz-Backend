using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Tests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(CreateTestTemplate, "/Templates");
    }

    public async Task<Ok<ApiResponse<Guid>>> CreateTestTemplate([FromBody] CreateTestTemplateCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
