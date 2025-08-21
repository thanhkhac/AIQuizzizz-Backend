using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Comments.Service;

public interface ICommentService
{
    public Task<bool> CanComment(Guid questionSetId);
    public Task<bool> CanDelete(Guid commentId, CancellationToken cancellationToken);
    public Task<bool> CanComment(QuestionSet questionSet);
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
    
    public async Task<bool> CanComment(Guid questionSetId)
    {
        var questionSet = await _context.QuestionSets
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync(qs => qs.Id == questionSetId);

        if (questionSet == null)
            return false;

        return await CanComment(questionSet);
    }

    public async Task<bool> CanDelete(Guid commentId, CancellationToken cancellationToken)
    {
        var isAdmin = await _identityService.IsInAnyRoleAsync(_user.UserId!.Value, Domain.Constants.Roles.Administrator,
            Domain.Constants.Roles.Moderator);

        if (isAdmin)
            return true;
        
        var canDeleteComment = await _context.Comments
            .Where(x => x.Id == commentId && x.CreatedBy.Equals(_user.UserId))
            .FirstOrDefaultAsync(cancellationToken);
        
        if (canDeleteComment == null)
            throw new ErrorCodeException(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_COMMENT, "User không có quyền xóa comment này");
        
        return true;
    }

    public async Task<bool> CanComment(QuestionSet questionSet)
    {
        if (questionSet.VisibilityMode == QuestionSetVisibilityMode.Public)
            return true;

        if (_user.UserId == null) return false;

        if (await _identityService.IsInAnyRoleAsync(_user.UserId.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator))
            return true;
        
        var hasShareAccess = await _context.QuestionSetUsers
            .AnyAsync(qsu => qsu.QuestionSetId == questionSet.Id && qsu.UserId == _user.UserId.Value);

        if (hasShareAccess)
            return true;
        
        // if (questionSet.VisibilityMode == QuestionSetVisibilityMode.OnlyClass)
        // {
        //     var classIds = _context.ClassQuestionSets.Where(x => x.QuestionSetId == questionSet.Id).Select(cqs => cqs.ClassId).ToList();
        //
        //     return await _context.ClassUsers
        //         .AnyAsync(cu => classIds.Contains(cu.ClassId) && cu.UserId == _user.UserId.Value);
        // }
        
        hasShareAccess = await (
            from cu in _context.ClassUsers
            join cqs in _context.ClassQuestionSets
                on cu.ClassId equals cqs.ClassId
            where cu.UserId == _user.UserId.Value && cqs.QuestionSetId == questionSet.Id
            select cu
        ).AnyAsync();
        
        if (hasShareAccess)
            return true;

        return false;
    }
}
