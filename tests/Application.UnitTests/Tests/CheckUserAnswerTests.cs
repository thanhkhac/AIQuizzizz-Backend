using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Tests.Dto;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.UnitTests.Tests;

public class CheckUserAnswerTests
{
    private static readonly Guid A = Guid.NewGuid(), B = Guid.NewGuid(), C = Guid.NewGuid();
    private static readonly Guid L1 = Guid.NewGuid(), L2 = Guid.NewGuid(), R1 = Guid.NewGuid(), R2 = Guid.NewGuid();

    private static QuestionResponseDto MultipleChoice(float score = 10) => new()
    {
        Score = score,
        QuestionData = new QuestionDataDto
        {
            MultipleChoice =
            [
                new MultipleChoiceItemDto { Id = A, Text = "a", IsAnswer = true },
                new MultipleChoiceItemDto { Id = B, Text = "b", IsAnswer = true },
                new MultipleChoiceItemDto { Id = C, Text = "c", IsAnswer = false },
            ]
        }
    };

    private static QuestionResponseDto Matching(float score = 10) => new()
    {
        Score = score,
        QuestionData = new QuestionDataDto
        {
            Matching = new MatchingDataDto
            {
                Matches = [new MatchDto { LeftId = L1, RightId = R1 }, new MatchDto { LeftId = L2, RightId = R2 }]
            }
        }
    };

    private static UserAnswerDto Answer(UserAnswerDataDto data) => new() { UserAnswerData = data };

    [Test]
    public void MultipleChoice_Partial_DuplicateIds_DoNotInflateScore()
    {
        var score = CheckUserAnswer.CheckMultipleChoiceAnswer(
            Answer(new UserAnswerDataDto { MultipleChoice = [A, A, A, A, A, A] }), MultipleChoice(), GradeQuestionMethod.Partial);

        score.Should().Be(5); // 1/2 đáp án đúng
    }

    [Test]
    public void MultipleChoice_Partial_NeverExceedsQuestionScore()
    {
        var score = CheckUserAnswer.CheckMultipleChoiceAnswer(
            Answer(new UserAnswerDataDto { MultipleChoice = [A, B, A, B] }), MultipleChoice(), GradeQuestionMethod.Partial);

        score.Should().Be(10);
    }

    [Test]
    public void MultipleChoice_Partial_WrongChoicePenalised()
    {
        var score = CheckUserAnswer.CheckMultipleChoiceAnswer(
            Answer(new UserAnswerDataDto { MultipleChoice = [A, C] }), MultipleChoice(), GradeQuestionMethod.Partial);

        score.Should().Be(0);
    }

    [Test]
    public void MultipleChoice_AllOrNothing_ExactSetRequired()
    {
        CheckUserAnswer.CheckMultipleChoiceAnswer(Answer(new UserAnswerDataDto { MultipleChoice = [B, A] }), MultipleChoice(),
            GradeQuestionMethod.AllOrNothing).Should().Be(10);
        CheckUserAnswer.CheckMultipleChoiceAnswer(Answer(new UserAnswerDataDto { MultipleChoice = [A] }), MultipleChoice(),
            GradeQuestionMethod.AllOrNothing).Should().Be(0);
    }

    [Test]
    public void Matching_DuplicatePairs_DoNotGiveFullScore()
    {
        var data = new UserAnswerDataDto
        {
            Matching = [new UserMatchingAnswerDto { LeftId = L1, RightId = R1 }, new UserMatchingAnswerDto { LeftId = L1, RightId = R1 }]
        };

        CheckUserAnswer.CheckMatchingAnswer(Answer(data), Matching(), GradeQuestionMethod.AllOrNothing).Should().Be(0);
        CheckUserAnswer.CheckMatchingAnswer(Answer(data), Matching(), GradeQuestionMethod.Partial).Should().Be(5);
    }

    [Test]
    public void Matching_AllCombinations_DoNotGiveFullScore()
    {
        var data = new UserAnswerDataDto
        {
            Matching =
            [
                new UserMatchingAnswerDto { LeftId = L1, RightId = R2 }, new UserMatchingAnswerDto { LeftId = L1, RightId = R1 },
                new UserMatchingAnswerDto { LeftId = L2, RightId = R1 }, new UserMatchingAnswerDto { LeftId = L2, RightId = R2 },
            ]
        };

        // chỉ cặp đầu tiên của mỗi item trái được tính -> (L1,R2) sai, (L2,R1) sai
        CheckUserAnswer.CheckMatchingAnswer(Answer(data), Matching(), GradeQuestionMethod.Partial).Should().Be(0);
        CheckUserAnswer.CheckMatchingAnswer(Answer(data), Matching(), GradeQuestionMethod.AllOrNothing).Should().Be(0);
    }

    [Test]
    public void Matching_Correct_FullScore()
    {
        var data = new UserAnswerDataDto
        {
            Matching = [new UserMatchingAnswerDto { LeftId = L2, RightId = R2 }, new UserMatchingAnswerDto { LeftId = L1, RightId = R1 }]
        };

        CheckUserAnswer.CheckMatchingAnswer(Answer(data), Matching(), GradeQuestionMethod.AllOrNothing).Should().Be(10);
    }

    [TestCase("Paris", "Paris")]
    [TestCase("Paris", "  paris ")]
    [TestCase("Hồ Chí Minh", "hồ   chí minh")]
    public void ShortText_IgnoresCaseAndExtraWhitespace(string key, string answer)
    {
        var question = new QuestionResponseDto { Score = 2, QuestionData = new QuestionDataDto { ShortText = key } };

        CheckUserAnswer.CheckShortTextAnswer(Answer(new UserAnswerDataDto { ShortText = answer }), question).Should().Be(2);
    }

    [Test]
    public void NullAnswerData_ReturnsZero_NoException()
    {
        var answer = new UserAnswerDto { UserAnswerData = null! };

        CheckUserAnswer.CheckMultipleChoiceAnswer(answer, MultipleChoice(), GradeQuestionMethod.Partial).Should().Be(0);
        CheckUserAnswer.CheckMatchingAnswer(answer, Matching(), GradeQuestionMethod.Partial).Should().Be(0);
        CheckUserAnswer.CheckShortTextAnswer(answer, new QuestionResponseDto { QuestionData = new QuestionDataDto { ShortText = "x" } })
            .Should().Be(0);
    }
}
