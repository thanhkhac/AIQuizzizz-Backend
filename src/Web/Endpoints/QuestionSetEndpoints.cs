using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class QuestionSetEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(CreateQuestionSet)
            .MapGet(GetPermissions, "{questionSetId}/Permissions")
            .MapGet(GetQuestions, "{questionSetId}/Questions")
            .MapPatch("{questionSetId}", UpdateQuestionSet)
            ;
    }

    public async Task<Ok<ApiResponse<Guid>>> CreateQuestionSet([FromBody] CreateQuestionSetCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Guid>>> UpdateQuestionSet(
        [FromRoute] Guid questionSetId,
        [FromBody] UpdateQuestionSetCommand command,
        ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<QuestionSetPermissionsDto>>> GetPermissions([FromRoute] Guid questionSetId, ISender sender)
    {
        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<List<QuestionResponseDto>>>> GetQuestions([FromRoute] Guid questionSetId, ISender sender)
    {
        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }


}
