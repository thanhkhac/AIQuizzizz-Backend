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
            .MapGet(GetQuestions, "{questionSetId}/Questions")
            .MapPatch(UpdateQuestionSet, "{questionSetId}")
            .MapDelete(DeleteQuestionSet, "{questionSetId}")
            .MapGet(GetPermissions, "{questionSetId}/Permissions")
            .MapGet(GetLearnQuestions, "{questionSetId}/LearnQuestions")
            .MapPost(UpdateQuestionSetHistory, "{questionSetId}/LearnHistory")
            .MapDelete(ResetQuestionSetHistory, "{questionSetId}/LearnHistory")
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

    public async Task<Ok<ApiResponse<Unit>>> UpdateQuestionSetHistory(
        [FromRoute] Guid questionSetId,
        [FromBody] List<QuestionHistoryUpdate> questions,
        ISender sender)
    {
        var command = new UpdateQuestionSetHistoryCommand
        {
            QuestionSetId = questionSetId,
            Questions = questions
        };

        var result = await sender.Send(command);
        return result.ToOk();
    }

    public async Task<Ok<ApiResponse<Unit>>> ResetQuestionSetHistory(
        [FromRoute] Guid questionSetId,
        ISender sender)
    {
        var command = new ResetQuestionSetHistoryCommand
        {
            QuestionSetId = questionSetId
        };

        var result = await sender.Send(command);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<List<QuestionResponseDto>>>> GetLearnQuestions(
        [FromRoute] Guid questionSetId,
        [FromQuery] int questionCount,
        ISender sender)
    {
        var query = new GetQuestionSetLearnQuestionsQuery
        {
            QuestionSetId = questionSetId,
            QuestionCount = questionCount
        };

        var result = await sender.Send(query);
        return result.ToOk();
    }


    public async Task<Ok<ApiResponse<Unit>>> DeleteQuestionSet(
        [FromRoute] Guid questionSetId,
        ISender sender)
    {
        var command = new DeleteQuestionSetCommand
        {
            QuestionSetId = questionSetId
        };

        var result = await sender.Send(command);
        return result.ToOk();
    }


}
