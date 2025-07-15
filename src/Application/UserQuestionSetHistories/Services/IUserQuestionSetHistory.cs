namespace CleanArchitectureBase.Application.UserQuestionSetHistories.Services;

public interface IUserQuestionSetHistoryService
{
    Task DeleteHistoriesByUserAndSetAsync(Guid userId, Guid questionSetId, CancellationToken cancellationToken);
}
