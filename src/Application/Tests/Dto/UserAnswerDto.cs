using System.Text.Json;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.Dto;

public class UserAnswerDto
{
    public Guid QuestionId { get; set; }
    public UserAnswerDataDto UserAnswerData { get; set; } = null!;
}

public class TestResultDto
{
    public DateTimeOffset TimeStart;
    public DateTimeOffset TimeEnd;
    public float Score;
}

public static class Serializer
{
    public static string Serialize(UserAnswerDataDto dto)
    {
        return dto.Type switch
        {
            nameof(QuestionType.MultipleChoice) => JsonSerializer.Serialize(dto.MultipleChoice),
            nameof(QuestionType.Matching) => JsonSerializer.Serialize(dto.Matching),
            nameof(QuestionType.Ordering) => JsonSerializer.Serialize(dto.Ordering),
            nameof(QuestionType.ShortText) => JsonSerializer.Serialize(dto.ShortText),
            _ => throw new InvalidDataException($"Invalid question type: {dto.Type}")
        };
    }
}

public static class CheckUserAnswer
{
    public static float CheckMultipleChoiceAnswer(UserAnswerDto userAnswer, QuestionResponseDto question)
    {
        if (question.QuestionData?.MultipleChoice == null || userAnswer.UserAnswerData?.MultipleChoice == null)
            return 0;

        var correctAnswers = question.QuestionData.MultipleChoice
            .Where(x => x.IsAnswer)
            .Select(x => x.Id)
            .ToHashSet();

        var check = userAnswer.UserAnswerData.MultipleChoice.ToHashSet();
        
        float score = userAnswer.UserAnswerData.MultipleChoice.ToHashSet().SetEquals(correctAnswers)
            ? question.Score
            : 0;
        return score;
    }

    public static float CheckMatchingAnswer(UserAnswerDto userAnswer, QuestionResponseDto question)
    {
        if(question.QuestionData.Matching == null || userAnswer.UserAnswerData.Matching == null)
            return 0;

        var answers = question.QuestionData.Matching.Matches
            .Select(x => new HashSet<Guid>{x.LeftId, x.RightId})
            .ToList();
        
        if (answers.Count == 0)
            return 0;

        var userAnswers = userAnswer.UserAnswerData.Matching
            .Select(x => new HashSet<Guid>{x.LeftId, x.RightId})
            .ToList();
        
        var correctAnswers = userAnswers
            .Where(x => answers.Any(ua => ua.SetEquals(x)))
            .ToList();
        
        var scorePerMatch = question.Score / answers.Count;
        
        return correctAnswers.Count * scorePerMatch;
    }

    public static float CheckOrderingAnswer(UserAnswerDto userAnswer, QuestionResponseDto question)
    {
        if(question.QuestionData.Ordering == null || userAnswer.UserAnswerData.Ordering == null)
            return 0;

        var answers = question.QuestionData.Ordering
            .OrderBy(x => x.CorrectOrder)
            .Select(x => x.Id)
            .ToList();

        var userAnswers = userAnswer.UserAnswerData.Ordering
            .OrderBy(x => x.Order)
            .Select(x => x.ItemId)
            .ToList();
        
        var isCorrect = answers.SequenceEqual(userAnswers);
        
        return isCorrect ? question.Score : 0;
    }

    public static float CheckShortTextAnswer(UserAnswerDto userAnswer, QuestionResponseDto question)
    {
        if (question.QuestionData.ShortText == null || userAnswer.UserAnswerData.ShortText == null)
            return 0;
        
        return question.QuestionData.ShortText.Equals(userAnswer.UserAnswerData.ShortText) ? question.Score : 0;
    }
}
