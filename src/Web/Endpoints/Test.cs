using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.QuestionSets.Common;
using CleanArchitectureBase.Application.Tests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class Test : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(CreateTestTemplate, "/Templates")
            .MapPost(ImportFileTestTemplate, "/Templates/ImportFile");
    }

    public async Task<Ok<ApiResponse<Guid>>> CreateTestTemplate([FromBody] CreateTestTemplateCommand rq, ISender sender)
    {
        var result = await sender.Send(rq);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<List<CreateQuestionDto>>>> ImportFileTestTemplate(
        [FromForm] FileData fileData,
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
