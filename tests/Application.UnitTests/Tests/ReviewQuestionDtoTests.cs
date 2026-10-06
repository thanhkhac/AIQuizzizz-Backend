using System.Text.Json;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.UnitTests.Tests;

public class ReviewQuestionDtoTests
{
    private static Question ShortTextQuestion() => new()
    {
        Type = QuestionType.ShortText,
        QuestionText = "q",
        Score = 10,
        DataJson = JsonSerializer.Serialize(new QTypeShortAnswer { Answer = "paris" }),
    };

    private static AttemptQuestion Answer(Guid questionId, float score) => new()
    {
        Id = Guid.NewGuid(),
        AttemptId = Guid.NewGuid(),
        QuestionId = questionId,
        Score = score,
        DataJson = JsonSerializer.Serialize("paris"),
    };

    [Test]
    public void AnswersHidden_PerQuestionScoreIsNull_RegardlessOfCorrectness()
    {
        var q = ShortTextQuestion();

        var correct = ReviewQuestionDto.Mapper.FromEntity(q, Answer(q.Id, 10), isShowCorrectAnswer: false);
        var wrong = ReviewQuestionDto.Mapper.FromEntity(q, Answer(q.Id, 0), isShowCorrectAnswer: false);

        correct.Score.Should().BeNull();
        wrong.Score.Should().BeNull();
        correct.Score.Should().Be(wrong.Score); // không phân biệt được đúng/sai qua điểm
    }

    [Test]
    public void AnswersShown_PerQuestionScoreIsReturned()
    {
        var q = ShortTextQuestion();

        ReviewQuestionDto.Mapper.FromEntity(q, Answer(q.Id, 7.5f), isShowCorrectAnswer: true).Score.Should().Be(7.5f);
        ReviewQuestionDto.Mapper.FromEntity(q, null, isShowCorrectAnswer: true).Score.Should().Be(0);
    }
}
