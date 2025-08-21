using CleanArchitectureBase.Application.Command.UnitTests.TestDataUltils;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Comments;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Comments;

using static Testing;

public class CreateCommentCommandTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldCreateCommentSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = QuestionJsonTestData.ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(question);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });
        var command = new CreateCommentCommand
        {
            QuestionId = question.Id,
            Content = "This is a valid comment."
        };

        var commentId = await SendAsync(command);

        commentId.Should().NotBeEmpty();
        var createdComment = await FindAsync<Comment>(commentId);
        createdComment.Should().NotBeNull();
        createdComment!.Content.Should().Be(command.Content);
        createdComment.QuestionId.Should().Be(question.Id);
        createdComment.CreatedBy.Should().Be(userId);
    }

    public static class CreateCommentCommandTestCases
    {
        public static IEnumerable<TestCaseData> InvalidContentCases
        {
            get
            {
                yield return new TestCaseData("").SetName("Content_Empty");
                yield return new TestCaseData(" ").SetName("Content_Whitespace");
                yield return new TestCaseData(null).SetName("Content_Null");
                yield return new TestCaseData(new string('a', 1001)).SetName("Content_TooLong"); // giả sử max = 1000
                yield return new TestCaseData(new string('a', 999)).SetName("Content_UnderBoundaryLong"); // giả sử max = 1000
            }
        }
    }

    [Test, TestCaseSource(typeof(CreateCommentCommandTestCases), nameof(CreateCommentCommandTestCases.InvalidContentCases))]
    public async Task ShouldThrowError_WhenContentIsInvalid(string content)
    {
        var userId = await RunAsDefaultUserAsync();


        var command = new CreateCommentCommand
        {
            QuestionId = Guid.NewGuid(),
            Content = content
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
    
    [Test]
    public async Task ShouldThrowError_WhenIdIsInvalid()
    {
        var userId = await RunAsDefaultUserAsync();


        var command = new CreateCommentCommand
        {
            QuestionId = null,
            Content = "Hell"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionNotFound()
    {
        await RunAsDefaultUserAsync();
        var query = new CreateCommentCommand
        {
            QuestionId = Guid.NewGuid(),
            Content = "This is a comment."
        };
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_CAN_NOT_COMMENT);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserCannotComment()
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
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = QuestionJsonTestData.ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(question);

        await RunAsDefaultUserAsync();
        var command = new CreateCommentCommand
        {
            QuestionId = question.Id,
            Content = "This is a comment."
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_COMMENT);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(questionSet);
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuestionSetId = questionSet.Id,
            QuestionText = "Q1",
            Type = QuestionType.ShortText,
            Score = 1,
            DataJson = QuestionJsonTestData.ShortTextDataJson,
            TextFormat = TextFormat.PlainText
        };
        await AddAsync(question);


        var command = new CreateCommentCommand
        {
            QuestionId = question.Id,
            Content = "This is a comment."
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
