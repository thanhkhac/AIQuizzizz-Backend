using System.Text.Json;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Questions.Services;

public interface IQuestionService
{
    Task<List<QuestionResponseDto>> GetQuestionsBySetIdForDetailAndLearnAsync(Guid questionSetId, Guid? userId,
        CancellationToken cancellationToken = default);
}

public class QuestionService : IQuestionService
{
    private readonly IApplicationDbContext _context;

    public QuestionService(IApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<List<QuestionResponseDto>> GetQuestionsBySetIdForDetailAndLearnAsync(Guid questionSetId, Guid? userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || userId == null)
        {
            var questions = await _context.Questions
                .Where(q =>
                    q.QuestionSetId == questionSetId
                    && q.IsDeleted == false)
                .ToListAsync(cancellationToken);

            return questions
                .Select(q => QuestionResponseDto.Mapper.FromEntity(q, completed: false))
                .ToList();
        }
        {
            var query = from q in _context.Questions
                where q.QuestionSetId == questionSetId
                join h in _context.UserQuestionSetHistories
                    on new
                    {
                        QuestionId = q.Id,
                        UserId = userId.Value
                    }
                    equals new
                    {
                        h.QuestionId,
                        h.UserId
                    }
                    into gj
                from history in gj.DefaultIfEmpty() // LEFT JOIN
                select new
                {
                    Question = q,
                    IsCorrect = history != null && history.IsCorrect
                };

            var result = await query.ToListAsync(cancellationToken);

            return result
                .Select(x => QuestionResponseDto.Mapper.FromEntity(x.Question, x.IsCorrect))
                .ToList();
        }
    }


}
