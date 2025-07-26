using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Comments.Service;

public interface ICommentService
{
    Task CanComment(Guid questionId, CancellationToken cancellationToken);
    Task CanDelete(Guid commentId, CancellationToken cancellationToken);
}

public class CommentService : ICommentService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    
    public CommentService(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }
    
    public async Task CanComment(Guid questionId, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .Include(x => x.QuestionSet)
            .Where(x => x.Id == questionId).FirstOrDefaultAsync(cancellationToken);
        if (question == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_NOT_FOUND, "Question không tìm thấy");
        
        if(question.QuestionSet == null)
            throw new ErrorCodeException(ErrorCodes.QUESTION_CAN_NOT_COMMENT, "Question không thể comment");
        
        var questionSetUser = await _context.QuestionSetUsers
            .Where(x => x.QuestionSetId == question.QuestionSet.Id && x.UserId == _user.UserId)
            .FirstOrDefaultAsync(cancellationToken);
        if (questionSetUser == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET, "User không trong question set của question");
    }

    public async Task CanDelete(Guid commentId, CancellationToken cancellationToken)
    {
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
            Domain.Constants.Roles.Moderator);

        if (isAdmin)
            return;
        
        var canDeleteComment = await _context.Comments
            .Where(x => x.Id == commentId && x.CreatedBy.Equals(_user.UserId))
            .FirstOrDefaultAsync(cancellationToken);
        
        if (canDeleteComment == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_COMMENT, "User không có quyền xóa comment này");
    }
}
