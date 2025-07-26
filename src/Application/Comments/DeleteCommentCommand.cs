using CleanArchitectureBase.Application.Comments.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Comments;

public class DeleteCommentCommand :IRequest<Guid>
{
    public Guid CommentId { get; set; }
}

public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("QuestionID không được để trống");
    }
}

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICommentService _commentService;
    
    public DeleteCommentCommandHandler(IApplicationDbContext context, ICommentService commentService)
    {
        _context = context;
        _commentService = commentService;
    }
    
    public async Task<Guid> Handle(DeleteCommentCommand rq, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .Include(x => x.Question)
            .Where(x => x.Id == rq.CommentId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);
        if (comment == null)
            throw new ErrorCodeException(ErrorCodes.COMMENT_NOT_FOUND, "Không tìm thấy comment"); 
        
        await _commentService.CanDelete(rq.CommentId, cancellationToken);
        
        comment.IsDeleted = true;
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return comment.Id;
    }
}
