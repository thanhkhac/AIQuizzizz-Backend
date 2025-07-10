using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.QuestionSets.Services;

public interface IQuestionSetService
{
    public Task<bool> CanUserEditQuestionSet(Guid userId, Guid questionSetId);
    public Task<bool> CanUserViewQuestionSet(Guid userId, Guid questionSetId);
    public Task<bool> CanUserDeleteQuestionSet(Guid userId, Guid questionSetId);
}

public class QuestionSetService : IQuestionSetService
{
    private readonly IApplicationDbContext _context;

    public QuestionSetService(IApplicationDbContext context)
    {
        _context = context;
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
    public async Task<bool> CanUserViewQuestionSet(Guid userId, Guid questionSetId)
    {
        var questionSet = await _context.QuestionSets
            .Include(qs => qs.QuestionSetUsers)
            .Include(qs => qs.ClassQuestionSets)
            .FirstOrDefaultAsync(qs => qs.Id == questionSetId);

        if (questionSet == null)
            return false;

        // Public question sets can be viewed by anyone
        if (questionSet.VisibilityMode == QuestionSetVisibilityMode.Public)
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

}
