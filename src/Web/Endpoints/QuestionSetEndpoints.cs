using CleanArchitectureBase.Application.Common.Models;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Web.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace CleanArchitectureBase.Web.Endpoints;

public class QuestionSetEndpoints : EndpointGroupBase
{

    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost(CreateQuestionSet)
            .MapGet(GetQuestionSetDetail, ("{questionSetId}"))
            .MapGet(GetQuestions, "{questionSetId}/Questions")
            .MapPatch(UpdateQuestionSet, "{questionSetId}")
            .MapDelete(DeleteQuestionSet, "{questionSetId}")
            .MapGet(GetLearnQuestions, "{questionSetId}/LearnQuestions")
            .MapPost(UpdateQuestionSetHistory, "{questionSetId}/LearnHistory")
            .MapDelete(ResetQuestionSetHistory, "{questionSetId}/LearnHistory")
            .MapGet(GetPermissions, "{questionSetId}/Permissions")
            .MapGet(GetQuestionsForEdit, "{questionSetId}/QuestionsForEdit")
            .MapGet(GetQuestionsForCopy, "{questionSetId}/GetQuestionsForCopy")
            ;
    }

    /// <summary>
    /// Create new question set
    /// </summary>
    /// <param name="command"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<Guid>>> CreateQuestionSet([FromBody] CreateQuestionSetCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.ToOk();
    }

    /// <summary>
    /// User (Owner or sharedmode is editable) Update question set
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="command"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<Guid>>> UpdateQuestionSet(
        [FromRoute] Guid questionSetId,
        [FromBody] UpdateQuestionSetCommand command,
        ISender sender)
    {
        command.QuestionSetId = questionSetId;
        var result = await sender.Send(command);
        return result.ToOk();
    }

    /// <summary>
    /// User - Get permission to show delete and update button
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<QuestionSetPermissionsDto>>> GetPermissions([FromRoute] Guid questionSetId, ISender sender)
    {
        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// User - Get questions of the question set
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<List<QuestionResponseDto>>>> GetQuestions([FromRoute] Guid questionSetId, ISender sender)
    {
        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Get questions data of the question set for editing
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<List<CreateUpdateQuestionDto>>>> GetQuestionsForEdit([FromRoute] Guid questionSetId, ISender sender)
    {
        var query = new GetQuestionSetQuestionsForEditQuery
        {
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Get questions data of the question set for copy/import 
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<List<CreateUpdateQuestionDto>>>> GetQuestionsForCopy([FromRoute] Guid questionSetId, ISender sender)
    {
        var query = new GetQuestionSetQuestionsForCopyQuery
        {
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// Get question detail data (No questions)
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<QuestionSetDetailDto>>> GetQuestionSetDetail([FromRoute] Guid questionSetId, ISender sender)
    {
        var query = new GetQuestionSetDetailQuery
        {
            QuestionSetId = questionSetId
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }

    /// <summary>
    /// User - mark questions that correct or fail in learn mode
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="questions"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
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

    /// <summary>
    /// User - Reset learn mode
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Get question for learn
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="questionCount"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task<Ok<ApiResponse<GetQuestionSetLearnQuestionsQueryDto>>> GetLearnQuestions(
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

    /// <summary>
    /// Delete a question set that current user own.
    /// </summary>
    /// <param name="questionSetId"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
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


    /// <summary>
    /// Search question sets by name/description (with pagination)
    /// </summary>
    public async Task<Ok<ApiResponse<PaginatedList<SearchQuestionSetDto>>>> SearchQuestionSets(
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string? name = null,
        [FromQuery] Guid? tagId = null)
    {
        var query = new SearchPublicQuestionSetByNameQuery
        {
            Name = name,
            TagId = tagId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await sender.Send(query);
        return result.ToOk();
    }


}
