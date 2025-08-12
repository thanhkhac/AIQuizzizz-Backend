using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class GetQuestionSetLearnQuestionsQueryTests : BaseTestFixture
{
    private const string ShortTextDataJson = "{\"Answer\":\"Thomas Edison\"}";

    private const string OrderingDataJson =
        "[{\"Id\":\"c89ece70-ace7-4773-8856-e54073dd21d0\",\"Text\":\"Sao Kim\",\"CorrectOrder\":1,\"ShuffleOrder\":0},{\"Id\":\"0992dd4c-3edb-4667-9ff4-2f95ee56257f\",\"Text\":\"Tr\\u00E1i \\u0110\\u1EA5t\",\"CorrectOrder\":2,\"ShuffleOrder\":1},{\"Id\":\"54648c68-4f9f-4fa4-9c99-70e841d7b7f1\",\"Text\":\"Sao Th\\u1EE7y\",\"CorrectOrder\":0,\"ShuffleOrder\":2}]";

    private const string MatchingDataJson =
        "[{\"Id\":\"b9c2030d-210f-4667-95f4-8dc5e05910c4\",\"Text\":\"Vi\\u1EC7t Nam\",\"AnswerId\":null,\"ShuffleOrder\":0},{\"Id\":\"ec1e75a6-6c30-4c1b-8689-c897502d1513\",\"Text\":\"H\\u00E0 N\\u1ED9i\",\"AnswerId\":\"b9c2030d-210f-4667-95f4-8dc5e05910c4\",\"ShuffleOrder\":1},{\"Id\":\"952e3bdb-9e33-42fe-b80e-b0f6045d92a2\",\"Text\":\"Paris\",\"AnswerId\":\"f7248e47-2516-4956-a0f1-a8f3dc971e7a\",\"ShuffleOrder\":2},{\"Id\":\"f7248e47-2516-4956-a0f1-a8f3dc971e7a\",\"Text\":\"Ph\\u00E1p\",\"AnswerId\":null,\"ShuffleOrder\":3}]";

    private const string MultipleChoiceDataJson =
        "[{\"Id\":\"6efb008f-fb47-4a91-a638-41cda932de41\",\"Text\":\"V\\u00E0ng\",\"IsAnswer\":false,\"ShuffleOrder\":0},{\"Id\":\"70fe01c6-8154-4db3-8cca-ba5e44ea5fd6\",\"Text\":\"\\u0110\\u1ECF\",\"IsAnswer\":false,\"ShuffleOrder\":1},{\"Id\":\"9f583bb3-2496-4b2c-837a-89dc3cd0313a\",\"Text\":\"Xanh d\\u01B0\\u01A1ng\",\"IsAnswer\":true,\"ShuffleOrder\":2}]";

    //normal
    [Test]
    public async Task ShouldReturnLearnQuestions_WhenValidRequest()
    {
        // Arrange
        var userId = await RunAsUserWithPlanAsync(); // User can learn
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId,
            QuestionCount = 5
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        // Simulate questions in the set with specific DataJson
        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1 (ShortText)",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        var q2 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q2 (Ordering)",
            Type = QuestionType.Ordering,
            Score = 1,
            DataJson = OrderingDataJson,
            TextFormat = TextFormat.PlainText
        };
        var q3 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q3 (Matching)",
            Type = QuestionType.Matching,
            Score = 1,
            DataJson = MatchingDataJson,
            TextFormat = TextFormat.PlainText
        };
        var q4 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q4 (MultipleChoice)",
            Type = QuestionType.MultipleChoice,
            Score = 1,
            DataJson = MultipleChoiceDataJson,
            TextFormat = TextFormat.PlainText
        };
        var q5 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q5 (ShortText)",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q1);
        await AddAsync(q2);
        await AddAsync(q3);
        await AddAsync(q4);
        await AddAsync(q5);

        // Simulate completed questions
        await AddAsync(new UserQuestionSetHistory
        {
            UserId = userId,
            QuestionId = q1.Id,
            IsCorrect = true
        });
        await AddAsync(new UserQuestionSetHistory
        {
            UserId = userId,
            QuestionId = q2.Id,
            IsCorrect = true
        });
        await AddAsync(new UserQuestionSetHistory
        {
            UserId = userId,
            QuestionId = q3.Id,
            IsCorrect = false
        });

        var query = new GetQuestionSetLearnQuestionsQuery
        {
            QuestionSetId = questionSet.Id,
            QuestionCount = 5
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.CompletedQuestionCount.Should().Be(2);
        result.TotalQuestionCount.Should().Be(5);
        result.Questions.Should().HaveCount(3);
        result.Questions.Should().Contain(q => q.Id == q3.Id);
        result.Questions.Should().Contain(q => q.Id == q4.Id);
        result.Questions.Should().Contain(q => q.Id == q5.Id);
    }

    //abnormal
    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public async Task ShouldThrowError_WhenQuestionCountIsZeroOrLess(int questionCount)
    {
        // Arrange
        var userId = await RunAsUserWithPlanAsync(); // User can learn
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new GetQuestionSetLearnQuestionsQuery
        {
            QuestionSetId = questionSet.Id,
            QuestionCount = questionCount
        };

        // Act & Assert
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionCountExceedsMax()
    {
        // Arrange
        var userId = await RunAsUserWithPlanAsync(); // User can learn
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new GetQuestionSetLearnQuestionsQuery
        {
            QuestionSetId = questionSet.Id,
            QuestionCount = 11
        }; // Max is 10

        // Act & Assert
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserCannotLearn()
    {
        // Arrange
        var userId = await RunAsDefaultUserAsync(); // User cannot learn
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var query = new GetQuestionSetLearnQuestionsQuery
        {
            QuestionSetId = questionSet.Id,
            QuestionCount = 5
        };

        // Act & Assert
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_REQUIRE_PLAN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        // Arrange
        await RunAsUserWithPlanAsync(); // User can learn
        var query = new GetQuestionSetLearnQuestionsQuery
        {
            QuestionSetId = Guid.NewGuid(),
            QuestionCount = 5
        }; // Non-existent ID

        // Act & Assert
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserCannotViewQuestionSet()
    {
        // Arrange
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        await RunAsUserWithPlanAsync();

        var query = new GetQuestionSetLearnQuestionsQuery
        {
            QuestionSetId = questionSet.Id,
            QuestionCount = 5
        };

        // Act & Assert
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        // Arrange
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(questionSet);

        // Do not call RunAsDefaultUserAsync(); // User is not logged in

        var query = new GetQuestionSetLearnQuestionsQuery
        {
            QuestionSetId = questionSet.Id,
            QuestionCount = 5
        };

        // Act & Assert
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
