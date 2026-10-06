using CleanArchitectureBase.Application.Questions.Dtos;
using CleanArchitectureBase.Application.Questions.Utils;
using CleanArchitectureBase.Application.QuestionSets.Dtos;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.UnitTests.Questions;

public class QuestionOrderingTests
{
    private static Question Build(CreateUpdateQuestionDto dto) => new()
    {
        Type = Enum.Parse<QuestionType>(dto.Type!),
        QuestionText = "q",
        DataJson = CreateUpdateQuestionDto.Serializer.Serialize(dto)
    };

    [Test]
    public void MultipleChoice_AuthorOrderIsPreservedAfterSave()
    {
        var texts = new[] { "4", "3", "5", "22/7", "x", "y", "z" };
        var dto = new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            MultipleChoices = texts.Select((t, i) => new CreateMultipleChoiceDto { Text = t, IsAnswer = i == 0 }).ToList()
        };

        for (var run = 0; run < 20; run++) // ShuffleOrder is random, author order must not be
        {
            var question = Build(dto);

            CreateUpdateQuestionDto.Deserializer.Deserialize(question).MultipleChoices!
                .Select(x => x.Text).Should().Equal(texts);

            QuestionDataDto.Deserializer.FromJson(question.Type, question.DataJson, shuffle: false, isShowAnswer: true)
                .MultipleChoice!.Select(x => x.Text).Should().Equal(texts);
        }
    }

    [Test]
    public void MultipleChoice_DeliveryOrderUsesShuffleOrder()
    {
        var dto = new CreateUpdateQuestionDto
        {
            Type = "MultipleChoice",
            MultipleChoices = Enumerable.Range(0, 8)
                .Select(i => new CreateMultipleChoiceDto { Text = i.ToString(), IsAnswer = i == 0 }).ToList()
        };
        var question = Build(dto);

        var delivered = QuestionDataDto.Deserializer.FromJson(question.Type, question.DataJson, shuffle: true, isShowAnswer: true)
            .MultipleChoice!;

        delivered.Should().HaveCount(8);
        delivered.Select(x => x.Text).OrderBy(x => x).Should().Equal(Enumerable.Range(0, 8).Select(i => i.ToString()).OrderBy(x => x));
    }

    [Test]
    public void Matching_PairOrderIsPreserved()
    {
        var pairs = new[] { ("a", "1"), ("b", "2"), ("c", "3"), ("d", "4") };
        var dto = new CreateUpdateQuestionDto
        {
            Type = "Matching",
            MatchingPairs = pairs.Select(p => new CreateMatchingPairDto { LeftItem = p.Item1, RightItem = p.Item2 }).ToList()
        };
        var question = Build(dto);

        CreateUpdateQuestionDto.Deserializer.Deserialize(question).MatchingPairs!
            .Select(p => $"{p.LeftItem}|{p.RightItem}").Should().Equal(pairs.Select(p => $"{p.Item1}|{p.Item2}"));

        var data = QuestionDataDto.Deserializer.FromJson(question.Type, question.DataJson, shuffle: false, isShowAnswer: true).Matching!;
        data.LeftItems.Select(x => x.Text).Should().Equal("a", "b", "c", "d");
        data.RightItems.Select(x => x.Text).Should().Equal("1", "2", "3", "4");
    }

    [Test]
    public void Ordering_AuthorOrderPreserved_CorrectOrderUntouched()
    {
        var dto = new CreateUpdateQuestionDto
        {
            Type = "Ordering",
            OrderingItems =
            [
                new CreateOrderingItemDto { Text = "third", CorrectOrder = 2 },
                new CreateOrderingItemDto { Text = "first", CorrectOrder = 0 },
                new CreateOrderingItemDto { Text = "second", CorrectOrder = 1 },
            ]
        };
        var question = Build(dto);

        var items = CreateUpdateQuestionDto.Deserializer.Deserialize(question).OrderingItems!;
        items.Select(x => x.Text).Should().Equal("third", "first", "second");
        items.Select(x => x.CorrectOrder).Should().Equal(2, 0, 1);
    }

    [Test]
    public void LegacyData_WithoutPosition_FallsBackToArrayOrder()
    {
        var json = """[{"Id":"11111111-1111-1111-1111-111111111111","Text":"b","IsAnswer":true,"ShuffleOrder":0},{"Id":"22222222-2222-2222-2222-222222222222","Text":"a","IsAnswer":false,"ShuffleOrder":1}]""";

        QuestionDataDto.Deserializer.FromJson(QuestionType.MultipleChoice, json, shuffle: false, isShowAnswer: true)
            .MultipleChoice!.Select(x => x.Text).Should().Equal("b", "a");
    }

    [Test]
    public void AuthorOrder_IsStableForTies()
    {
        var items = new[] { (1, "x"), (0, "y"), (1, "z"), (0, "w") };
        QuestionOrderHelper.AuthorOrder(items, i => i.Item1).Select(i => i.Item2).Should().Equal("y", "w", "x", "z");
    }
}
