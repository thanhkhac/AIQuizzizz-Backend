using System.Text.Json;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Questions.Services;

public interface IQuestionService
{
    Task<List<QuestionResponseDto>> GetQuestionsBySetIdForDetailAsync(Guid questionSetId, Guid? userId,
        CancellationToken cancellationToken = default);
        
    public Task<List<QuestionResponseDto>> GetQuestionsBySetIdForLearnAsync(Guid questionSetId, Guid userId, int questionCount,
        CancellationToken cancellationToken = default);
}

public class QuestionService : IQuestionService
{
    private readonly IApplicationDbContext _context;

    public QuestionService(IApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<List<QuestionResponseDto>> GetQuestionsBySetIdForDetailAsync(Guid questionSetId, Guid? userId,
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
                .Select(q => QuestionResponseDto.Mapper.FromEntity(q, isCorrect: null))
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
                    IsCorrect = history != null ? history.IsCorrect : (bool?)null
                };

            var result = await query.ToListAsync(cancellationToken);

            return result
                .Select(x => QuestionResponseDto.Mapper.FromEntity(x.Question, x.IsCorrect))
                .ToList();
        }
    }

    public async Task<List<QuestionResponseDto>> GetQuestionsBySetIdForLearnAsync(Guid questionSetId, Guid userId, int questionCount,
        CancellationToken cancellationToken = default)
    {
    
        var query = _context.Questions
            .Where(q => q.QuestionSetId == questionSetId && !q.IsDeleted)
            .GroupJoin(_context.UserQuestionSetHistories,
                q => new { QuestionId = q.Id, UserId = userId },
                h => new { h.QuestionId, h.UserId },
                (q, gj) => new { Question = q, History = gj })
            .SelectMany(x => x.History.DefaultIfEmpty(),
                (q, history) => new { q.Question, History = history })
            .Where(x => x.History == null || x.History.IsCorrect == false)
            .OrderBy(x => x.History != null && x.History.IsCorrect == false ? 0 : 1)
            .Select(x => new
            {
                Question = x.Question,
                IsCorrect = x.History != null ? x.History.IsCorrect : (bool?)null
            });

        var result = await query.Take(questionCount).ToListAsync(cancellationToken);

        return result
            .Select(x => QuestionResponseDto.Mapper.FromEntity(x.Question, x.IsCorrect))
            .ToList();
    }


}
