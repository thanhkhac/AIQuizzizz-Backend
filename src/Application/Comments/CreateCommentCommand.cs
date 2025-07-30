using CleanArchitectureBase.Application.Comments.Service;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Comments;

[Authorize]
public class CreateCommentCommand : IRequest<Guid>
{
    public Guid QuestionId { get; set; }
    public required string Content { get; set; }
}

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("QuestionID không được để trống");
        
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content không được để trống");
    }
}

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICommentService _commentService;
    
    public CreateCommentCommandHandler(
        IApplicationDbContext context,
        ICommentService commentService)
    {
        _context = context;
        _commentService = commentService;
    }
    
    public async Task<Guid> Handle(CreateCommentCommand rq, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .Include(x => x.QuestionSet)
            .Where(x => x.Id == rq.QuestionId && x.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);

        if (question == null || question.QuestionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_CAN_NOT_COMMENT,
                "Question không tồn tại hoặc không thể comment");
        
        var canComment = await _commentService.CanComment(question.QuestionSet.Id);
        if (!canComment)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_COMMENT, "User không được comment");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = rq.Content,
            QuestionId = rq.QuestionId,
            IsDeleted = false,
        };
        
        _context.Comments.Add(comment);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return comment.Id;
    }
}
