using CleanArchitectureBase.Application.Comments.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Comments;

[Authorize]
public class ReplyCommentCommand : IRequest<Guid>
{
    public Guid CommentId { get; set; }
    public required string Content { get; set; }
}

public class ReplyCommentCommandValidator : AbstractValidator<ReplyCommentCommand>
{
    public ReplyCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("QuestionID không được để trống");
        
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content không được để trống");
    }
}

public class ReplyCommentCommandHandler : IRequestHandler<ReplyCommentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICommentService _commentService;
    
    public ReplyCommentCommandHandler(IApplicationDbContext context, IUser user, ICommentService commentService)
    {
        _context = context;
        _commentService = commentService;
    }
    
    public async Task<Guid> Handle(ReplyCommentCommand rq, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .Include(x => x.Question)
            .ThenInclude(x => x!.QuestionSet)
            .Where(x => x.Id == rq.CommentId && x.IsDeleted == false
                                             && x.Question != null
                                             && x.Question.QuestionSet != null
                                             && x.Question.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (comment == null)
            throw new ErrorCodeException(ErrorCodes.COMMENT_NOT_FOUND, "Không tìm thấy comment");
        
        var canGetComment = await _commentService.CanComment(comment.Question!.QuestionSet!.Id);
        if (!canGetComment)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_COMMENT, "User không được comment");

        var reply = new Comment
        {
            Id = Guid.NewGuid(),
            Content = rq.Content,
            ParentId = rq.CommentId,
            QuestionId = comment.QuestionId,
            IsDeleted = false,
        };
        
        _context.Comments.Add(reply);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return reply.Id;
    }
}
