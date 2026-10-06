using System.Text.Json;
using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Tests.Dto;

public class UserAnswerDto
{
    public Guid QuestionId { get; set; }
    public UserAnswerDataDto UserAnswerData { get; set; } = null!;
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

    public static UserAnswerDataDto? DeSerialize(string type, string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }
        
        UserAnswerDataDto? questionData = type switch
        {
            nameof(QuestionType.MultipleChoice) => new UserAnswerDataDto()
            {
                MultipleChoice = JsonSerializer.Deserialize<List<Guid>>(json),
                Type = type,
            },
            nameof(QuestionType.Matching) => new UserAnswerDataDto()
            {
                Matching = JsonSerializer.Deserialize<List<UserMatchingAnswerDto>>(json),
                Type = type
            },
            nameof(QuestionType.Ordering) => new UserAnswerDataDto()
            {
                Ordering = JsonSerializer.Deserialize<List<UserOrderingAnswerDto>>(json),
                Type = type
            },
            nameof(QuestionType.ShortText) => new UserAnswerDataDto()
            {
                ShortText = !json.Equals("[]") ? JsonSerializer.Deserialize<string?>(json) : null,
                Type = type
            },
            _ => throw new InvalidDataException($"Invalid question type: {json}")
        };
        
        return questionData;
    }
}

public static class CheckUserAnswer
{
    public static float CheckMultipleChoiceAnswer(UserAnswerDto userAnswer, QuestionResponseDto question, GradeQuestionMethod gradeQuestionMethod)
    {
        if (question.QuestionData?.MultipleChoice == null || userAnswer.UserAnswerData?.MultipleChoice == null)
            return 0;

        var correctAnswers = question.QuestionData.MultipleChoice
            .Where(x => x.IsAnswer == true)
            .Select(x => x.Id)
            .ToHashSet();
        if (correctAnswers.Count == 0)
            return 0;

        // Bỏ ID trùng và ID không thuộc câu hỏi (tránh gửi [A,A,A] để nhân điểm)
        var validIds = question.QuestionData.MultipleChoice.Select(x => x.Id).ToHashSet();
        var picked = userAnswer.UserAnswerData.MultipleChoice.Where(validIds.Contains).ToHashSet();

        if (GradeQuestionMethod.Partial == gradeQuestionMethod)
        {
            var correctCount = picked.Count(correctAnswers.Contains);
            var inCorrectCount = picked.Count - correctCount;
            var partial = (correctCount - inCorrectCount) * (question.Score / correctAnswers.Count);
            return Math.Clamp(partial, 0, question.Score);
        }

        return picked.SetEquals(correctAnswers) ? question.Score : 0;
    }

    public static float CheckMatchingAnswer(UserAnswerDto userAnswer, QuestionResponseDto question, GradeQuestionMethod gradeQuestionMethod)
    {
        if (question.QuestionData?.Matching?.Matches == null || userAnswer.UserAnswerData?.Matching == null)
            return 0;

        var answers = question.QuestionData.Matching.Matches
            .Select(x => (x.LeftId, x.RightId))
            .ToHashSet();
        if (answers.Count == 0)
            return 0;

        // Mỗi item bên trái chỉ được ghép 1 lần (chặn gửi trùng cặp / gửi mọi tổ hợp)
        var userPairs = userAnswer.UserAnswerData.Matching
            .GroupBy(x => x.LeftId)
            .Select(g => (g.Key, g.First().RightId))
            .ToHashSet();

        if (GradeQuestionMethod.AllOrNothing == gradeQuestionMethod)
            return userPairs.SetEquals(answers) ? question.Score : 0;

        var correctCount = userPairs.Count(answers.Contains);
        return Math.Min(correctCount * (question.Score / answers.Count), question.Score);
    }

    public static float CheckOrderingAnswer(UserAnswerDto userAnswer, QuestionResponseDto question, GradeQuestionMethod gradeQuestionMethod)
    {
        if (question.QuestionData?.Ordering == null || userAnswer.UserAnswerData?.Ordering == null)
            return 0;

        var answers = question.QuestionData.Ordering
            .OrderBy(x => x.CorrectOrder)
            .Select(x => x.Id)
            .ToList();
        if (answers.Count == 0)
            return 0;

        var userAnswers = userAnswer.UserAnswerData.Ordering
            .OrderBy(x => x.Order)
            .Select(x => x.ItemId)
            .ToList();

        if (GradeQuestionMethod.Partial == gradeQuestionMethod)
        {
            var pointPerCorrect = question.Score / answers.Count;
            var countFor = Math.Min(userAnswers.Count, answers.Count);
            var totalPoint = 0f;
            for (var i = 0; i < countFor; i++)
            {
                if (answers[i].Equals(userAnswers[i]))
                    totalPoint += pointPerCorrect;
            }
            return Math.Min(totalPoint, question.Score);
        }

        return answers.SequenceEqual(userAnswers) ? question.Score : 0;
    }

    public static float CheckShortTextAnswer(UserAnswerDto userAnswer, QuestionResponseDto question)
    {
        if (question.QuestionData?.ShortText == null || userAnswer.UserAnswerData?.ShortText == null)
            return 0;

        return NormalizeShortText(question.QuestionData.ShortText) == NormalizeShortText(userAnswer.UserAnswerData.ShortText)
            ? question.Score
            : 0;
    }

    /// <summary>
    /// So sánh không phân biệt hoa thường, bỏ khoảng trắng thừa ("  Paris " == "paris")
    /// </summary>
    public static string NormalizeShortText(string text)
    {
        return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
    }
}
