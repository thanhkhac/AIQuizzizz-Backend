using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class GetQuestionSetQuestionsQueryTests : BaseTestFixture
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
    public async Task ShouldReturnQuestions_WhenUserIsOwner()
    {
        var userId = await RunAsDefaultUserAsync();
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

        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        var q2 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q2",
            Type = QuestionType.MultipleChoice,
            Score = 1,
            DataJson = MultipleChoiceDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q1);
        await AddAsync(q2);

        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(q => q.QuestionText == "Q1");
        result.Should().Contain(q => q.QuestionText == "Q2");
    }

    //normal
    [Test]
    public async Task ShouldReturnQuestions_WhenUserHasEditableShareMode()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        var editorId = await RunAsDefaultUserAsync();
        await AddAsync(new QuestionSetUser
        {
            UserId = editorId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Editable
        });

        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q1);

        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(q => q.QuestionText == "Q1");
    }

    //normal
    [Test]
    public async Task ShouldReturnQuestions_WhenUserHasViewOnlyShareMode()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        var viewerId = await RunAsDefaultUserAsync();
        await AddAsync(new QuestionSetUser
        {
            UserId = viewerId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q1);

        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(q => q.QuestionText == "Q1");
    }

    //normal
    [Test]
    public async Task ShouldReturnQuestions_WhenQuestionSetIsPublicAndUserIsLoggedIn()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Public Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        await AddAsync(questionSet);

        var userId = await RunAsDefaultUserAsync();
        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q1);

        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(q => q.QuestionText == "Q1");
    }

    //normal
    [Test]
    public async Task ShouldReturnQuestions_WhenQuestionSetIsPublicAndUserIsNotLoggedIn()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Public Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        await AddAsync(questionSet);

        var q1 = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(q1);

        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(q => q.QuestionText == "Q1");
    }


    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = Guid.NewGuid()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserCannotViewPrivateQuestionSet()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Private Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        var userId = await RunAsDefaultUserAsync();
        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserIsNotLoggedInAndQuestionSetIsPrivate()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Private Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);


        var query = new GetQuestionSetQuestionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }
}
