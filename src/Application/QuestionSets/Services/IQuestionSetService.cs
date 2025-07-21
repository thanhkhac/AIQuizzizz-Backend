using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.QuestionSets.Services;

public interface IQuestionSetService
{
    public Task<bool> CanUserEditQuestionSet(Guid userId, Guid questionSetId);
    public Task<bool> CanUserViewQuestionSet(Guid? userId, Guid questionSetId);
    public Task<bool> CanUserDeleteQuestionSet(Guid userId, Guid questionSetId);
    public Task<bool> CanUserViewQuestionSet(Guid? userId, QuestionSet questionSet);
    public Task<QuestionSetPermissionsDto> GetPermissions(Guid userId, Guid questionSetId);
    public Task<QuestionSet?> GetActiveQuestionSet(Guid questionSetId, CancellationToken cancellationToken);
}

public class QuestionSetService : IQuestionSetService
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public QuestionSetService(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    //Owner có thể edit hoặc được share trong bảng QuestionSetUser với QuestionSetUserShareMode Editable
    public async Task<bool> CanUserEditQuestionSet(Guid userId, Guid questionSetId)
    {
        return await _context.QuestionSetUsers
            .AnyAsync(qsu => qsu.QuestionSetId == questionSetId
                             && qsu.UserId == userId
                             && (qsu.ShareMode == QuestionSetUserShareMode.Owner
                                 || qsu.ShareMode == QuestionSetUserShareMode.Editable));
    }

    //Owner có thể edit hoặc được share trong bảng QuestionSetUser với bất kỳ QuestionSetUserShareMode nào
    //Hoặc là questionSet đó được hiển thị ở chế độ class
    //Hoặc questionSet đó public
    public async Task<bool> CanUserViewQuestionSet(Guid? userId, Guid questionSetId)
    {
        var questionSet = await _context.QuestionSets
            .Include(qs => qs.QuestionSetUsers)
            .Include(qs => qs.ClassQuestionSets)
            .Where(x => x.IsDeleted == false)
            .FirstOrDefaultAsync(qs => qs.Id == questionSetId);

        if (questionSet == null)
            return false;

        return await CanUserViewQuestionSet(userId, questionSet);
    }

    public async Task<bool> CanUserViewQuestionSet(Guid? userId, QuestionSet questionSet)
    {
        // Public question sets can be viewed by anyone
        if (questionSet.VisibilityMode == QuestionSetVisibilityMode.Public)
            return true;

        if (userId == null) return false;

        if (await _identityService.IsInAnyRoleAsync(userId.Value, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator))
            return true;

        // Kiểm tra xem người dùng được share quyền nào không
        var hasShareAccess = questionSet.QuestionSetUsers
            .Any(qsu => qsu.UserId == userId);

        if (hasShareAccess)
            return true;

        // Check theo class mode
        if (questionSet.VisibilityMode == QuestionSetVisibilityMode.OnlyClass)
        {
            var classIds = questionSet.ClassQuestionSets.Select(cqs => cqs.ClassId);

            return await _context.ClassUsers
                .AnyAsync(cu => classIds.Contains(cu.ClassId) && cu.UserId == userId);
        }
        return false;
    }

    //Owner có thể delete (Check trong bảng QuestionSetUserShareMode)
    public async Task<bool> CanUserDeleteQuestionSet(Guid userId, Guid questionSetId)
    {
        return await _context.QuestionSetUsers
            .AnyAsync(qsu => qsu.QuestionSetId == questionSetId
                             && qsu.UserId == userId
                             && qsu.ShareMode == QuestionSetUserShareMode.Owner);
    }

    public async Task<QuestionSetPermissionsDto> GetPermissions(Guid userId, Guid questionSetId)
    {
        var canEdit = false;
        bool canDelete = await _identityService.IsInAnyRoleAsync(userId, Domain.Constants.Roles.Administrator, Domain.Constants.Roles.Moderator);
        // Nếu là admin hoặc moderator thì có quyền delete

        // Nếu không phải admin thì kiểm tra theo bảng QuestionSetUsers
        var qsu = await _context.QuestionSetUsers
            .Where(q => q.QuestionSetId == questionSetId && q.UserId == userId)
            .Select(q => q.ShareMode)
            .FirstOrDefaultAsync();
        if (canDelete == false)
        {
            canDelete = qsu == QuestionSetUserShareMode.Owner;
        }
        if (canEdit == false)
        {
            canEdit = qsu == QuestionSetUserShareMode.Owner || qsu == QuestionSetUserShareMode.Editable;
        }
        return new QuestionSetPermissionsDto
        {
            CanEdit = canEdit,
            CanDelete = canDelete
        };
    }
    
    public async Task<QuestionSet?> GetActiveQuestionSet(Guid questionSetId, CancellationToken cancellationToken)
    {
        var questionSet = await _context.QuestionSets
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x =>
                x.Id == questionSetId
                && x.IsDeleted == false
                && x.CreatedByUser != null
                && x.CreatedByUser.IsDeleted == false
                && x.CreatedByUser.IsBanned == false, cancellationToken: cancellationToken);
        return questionSet;
    }


}
