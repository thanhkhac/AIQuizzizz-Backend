using CleanArchitectureBase.Application.Command.UnitTests.TestDataUltils;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Comments;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Comments;

using static Testing;

public class DeleteCommentCommandTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldSoftDeleteCommentSuccessfully()
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

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Content = "Comment to delete",
            IsDeleted = false
        };
        await AddAsync(comment);

        var command = new DeleteCommentCommand
        {
            CommentId = comment.Id
        };

        var deletedCommentId = await SendAsync(command);

        deletedCommentId.Should().Be(comment.Id);
        var deletedComment = await FindAsync<Comment>(comment.Id);
        deletedComment.Should().BeNull();
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenCommentIdIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var command = new DeleteCommentCommand
        {
            CommentId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenCommentNotFound()
    {
        await RunAsDefaultUserAsync();
        var command = new DeleteCommentCommand
        {
            CommentId = Guid.NewGuid()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMENT_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserCannotDeleteComment()
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
        await AddAsync(new QuestionSetUser
        {
            UserId = ownerId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Content = "Comment to delete",
            IsDeleted = false
        };
        await AddAsync(comment);

        await RunAsDefaultUserAsync();
        var command = new DeleteCommentCommand
        {
            CommentId = comment.Id
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


        var command = new DeleteCommentCommand
        {
            CommentId = question.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
