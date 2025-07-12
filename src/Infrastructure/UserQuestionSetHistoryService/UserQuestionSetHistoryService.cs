using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.UserQuestionSetHistories.Services;
using CleanArchitectureBase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Infrastructure.UserQuestionSetHistoryService;

public class UserQuestionSetHistoryService : IUserQuestionSetHistoryService
{
    private readonly IApplicationDbContext _context;

    public UserQuestionSetHistoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task DeleteHistoriesByUserAndSetAsync(Guid userId, Guid questionSetId, CancellationToken cancellationToken)
    {
        await _context.UserQuestionSetHistories
            .Where(h => h.UserId == userId)
            .Where(h => _context.Questions
                .Where(q => q.QuestionSetId == questionSetId)
                .Select(q => q.Id)
                .Contains(h.QuestionId))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
