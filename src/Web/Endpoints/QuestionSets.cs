using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class QuestionSets : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(CreateQuestionSet, "Create");
    }
    
    public async Task<Ok<ApiResponse<Guid>>> CreateQuestionSet([FromBody] CreateQuestionSetCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }
}
