using System.Text.Json;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Questions.Services;

public interface IQuestionService
{
    Task<List<QuestionResponseDto>> GetQuestionsBySetIdForDetailAndLearnAsync(Guid questionSetId, Guid userId,
        CancellationToken cancellationToken = default);
}

public class QuestionService : IQuestionService
{
    private readonly IApplicationDbContext _context;

    public QuestionService(IApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<List<QuestionResponseDto>> GetQuestionsBySetIdForDetailAndLearnAsync(Guid questionSetId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = from q in _context.Questions
            where q.QuestionSetId == questionSetId
            join h in _context.UserQuestionSetHistories
                on new
                {
                    QuestionId = q.Id,
                    UserId = userId
                }
                equals new
                {
                    h.QuestionId,
                    h.UserId
                } into gj
            from history in gj.DefaultIfEmpty() // Left join
            select new
            {
                Question = q,
                IsCorrect = history != null && history.IsCorrect
            };
        var result = await query.ToListAsync(cancellationToken);

        var response = new List<QuestionResponseDto>();
        foreach (var i in result)
        {
            var question = i.Question;
            var questionCompleted = i.IsCorrect;

            var questionData = DeserializeQuestionData(question.Type, question.DataJson);

            response.Add(new QuestionResponseDto
            {
                Id = question.Id,
                QuestionSetId = question.QuestionSetId ?? Guid.Empty,
                Type = question.Type.ToString(),
                TextFormat = question.TextFormat,
                QuestionText = question.QuestionText ?? string.Empty,
                Score = question.Score,
                Completed = questionCompleted,
                QuestionData = questionData
            });
        }

        return response;
    }

    private QuestionDataDto DeserializeQuestionData(QuestionType type, string? dataJson)
    {
        if (string.IsNullOrEmpty(dataJson))
            return new QuestionDataDto();

        var result = new QuestionDataDto();

        switch (type)
        {
            case QuestionType.MultipleChoice:
                var multipleChoiceItems = JsonSerializer.Deserialize<List<QTypeMultipleChoice>>(dataJson);
                result.MultipleChoice = multipleChoiceItems?.Select(item => new MultipleChoiceItemDto
                {
                    Id = item.Id,
                    Text = item.Text,
                    IsAnswer = item.IsAnswer
                }).ToList();
                break;

            case QuestionType.Matching:
                var matchingItems = JsonSerializer.Deserialize<List<QTypeMatching>>(dataJson);
                if (matchingItems != null)
                {
                    var matchingData = new MatchingDataDto();

                    var leftItems = matchingItems.Where(x => string.IsNullOrEmpty(x.AnswerId)).ToList();
                    var rightItems = matchingItems.Where(x => !string.IsNullOrEmpty(x.AnswerId)).ToList();

                    matchingData.LeftItems = leftItems.Select(item => new MatchingItemDto
                    {
                        Id = item.Id,
                        Text = item.Text
                    }).ToList();

                    matchingData.RightItems = rightItems.Select(item => new MatchingItemDto
                    {
                        Id = item.Id,
                        Text = item.Text
                    }).ToList();

                    matchingData.Matches = rightItems.Select(item => new MatchDto
                    {
                        LeftId = Guid.Parse(item.AnswerId!),
                        RightId = item.Id
                    }).ToList();

                    result.Matching = matchingData;
                }
                break;

            case QuestionType.Ordering:
                var orderingItems = JsonSerializer.Deserialize<List<QTypeOrderingItem>>(dataJson);
                result.Ordering = orderingItems?.Select(item => new OrderingItemDto
                {
                    Id = item.Id,
                    Text = item.Text,
                    CorrectOrder = item.CorrectOrder
                }).ToList();
                break;

            case QuestionType.ShortText:
                var shortAnswer = JsonSerializer.Deserialize<QTypeShortAnswer>(dataJson);
                result.ShortText = shortAnswer?.Answer;
                break;
        }

        return result;
    }


    // public async Task<List<QuestionResponseDto>> GetQuestionsBySetIdForDetailAndLearnAsyncV2(Guid questionSetId, Guid userId,
    //     CancellationToken cancellationToken = default)
    // {
    //     var questions = await _context.Questions
    //         .Where(q => q.QuestionSetId == questionSetId)
    //         .ToListAsync(cancellationToken);
    //
    //     var questionHistory = await _context.UserQuestionSetHistories
    //         .Where(h => h.UserId == userId &&
    //                     questions.Select(q => q.Id).Contains(h.QuestionId))
    //         .ToListAsync(cancellationToken);
    //
    //     var response = new List<QuestionResponseDto>();
    //
    //     foreach (var question in questions)
    //     {
    //         var questionCompleted = questionHistory
    //             .Any(h => h.QuestionId == question.Id && h.IsCorrect);
    //
    //         var questionData = DeserializeQuestionData(question.Type, question.DataJson);
    //
    //         response.Add(new QuestionResponseDto
    //         {
    //             Id = question.Id,
    //             QuestionSetId = question.QuestionSetId ?? Guid.Empty,
    //             Type = question.Type.ToString(),
    //             TextFormat = question.TextFormat,
    //             QuestionText = question.QuestionText ?? string.Empty,
    //             Score = question.Score,
    //             Completed = questionCompleted,
    //             QuestionData = questionData
    //         });
    //     }
    //
    //     return response;
    // }
}
