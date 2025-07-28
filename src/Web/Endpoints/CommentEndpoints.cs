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
        ISender sender)
    {    
        var rq = new GetCommentByQuestionQuery { QuestionId = questionId};
        var result = await sender.Send(rq);
        return result.ToOk();
    }
    
    public async Task<Ok<ApiResponse<Guid>>> CreateComment(
        [FromRoute] Guid questionId,
        [FromQuery] string content,
        ISender sender)
    {    
        var rq = new CreateCommentCommand { QuestionId = questionId, Content = content, };
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
        [FromQuery] string content,
        ISender sender)
    {    
        var rq = new ReplyCommentCommand { CommentId = commentId, Content = content};
        var result = await sender.Send(rq);
        return result.ToOk();
    }
}
