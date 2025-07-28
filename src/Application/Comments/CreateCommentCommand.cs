using CleanArchitectureBase.Application.Comments.Service;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
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
    
    public CreateCommentCommandHandler(IApplicationDbContext context, IUser user, ICommentService commentService)
    {
        _context = context;
        _commentService = commentService;
    }
    
    public async Task<Guid> Handle(CreateCommentCommand rq, CancellationToken cancellationToken)
    { 
        await _commentService.CanComment(rq.QuestionId, cancellationToken);

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
