using CleanArchitectureBase.Application.Comments;
using CleanArchitectureBase.Application.Comments.Dto;
using CleanArchitectureBase.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitectureBase.Web.Endpoints;

public class CommentEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetComment, "Question/{questionId}")
            .MapPost(CreateComment, "Question/{questionId}")
            .MapPost(ReplyComment, "{commentId}/Reply")
            .MapDelete(DeleteComment, "{commentId}");
    }
    
    public async Task<Ok<ApiResponse<PaginatedList<CommentDto>>>> GetComment(
        [FromRoute] Guid questionId,
        ISender sender,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {    
        var rq = new GetCommentByQuestionQuery { QuestionId = questionId, PageNumber = pageNumber, PageSize = pageSize};
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> CreateComment(
        [FromBody] CreateCommentCommand rq,
        [FromRoute] Guid questionId,
        ISender sender)
    {    
        rq.QuestionId = questionId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> DeleteComment(
        [FromRoute] Guid commentId,
        ISender sender)
    {    
        var rq = new DeleteCommentCommand { CommentId = commentId};
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> ReplyComment(
        [FromRoute] Guid commentId,
        [FromBody] ReplyCommentCommand rq,
        ISender sender)
    {    
        rq.CommentId = commentId;
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
