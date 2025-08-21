using CleanArchitectureBase.Application.Command.UnitTests.TestDataUltils;
using CleanArchitectureBase.Application.Comments;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.QuestionSets.Services;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Comments;

using static Testing;

public class ReplyCommentCommandTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReplyCommentSuccessfully()
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

        var parentComment = new Comment
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Content = "Parent comment",
            IsDeleted = false
        };
        await AddAsync(parentComment);

        var command = new ReplyCommentCommand
        {
            CommentId = parentComment.Id,
            Content = "This is a reply."
        };

        var replyId = await SendAsync(command);

        replyId.Should().NotBeEmpty();
        var replyComment = await FindAsync<Comment>(replyId);
        replyComment.Should().NotBeNull();
        replyComment!.Content.Should().Be(command.Content);
        replyComment.ParentId.Should().Be(parentComment.Id);
        replyComment.QuestionId.Should().Be(question.Id);
        replyComment.CreatedBy.Should().Be(userId);
    }


    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenContentIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = GetUserId()!.Value
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
            UserId = GetUserId()!.Value,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var parentComment = new Comment
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Content = "Parent comment",
            IsDeleted = false
        };
        await AddAsync(parentComment);

        var command = new ReplyCommentCommand
        {
            CommentId = parentComment.Id,
            Content = ""
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
    
    
    [Test, TestCaseSource(typeof(CreateCommentCommandTests.CreateCommentCommandTestCases), nameof(CreateCommentCommandTests.CreateCommentCommandTestCases.InvalidContentCases))]
    public async Task ShouldThrowError_WhenContentIsInvalid(string content)
    {
        var userId = await RunAsDefaultUserAsync();


        var command = new ReplyCommentCommand
        {
            CommentId = Guid.NewGuid(),
            Content = content
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
    
    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenContentIsNull()
    {
        await RunAsDefaultUserAsync();

        var command = new ReplyCommentCommand
        {
            CommentId = null,
            Content = "Hello"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenParentCommentNotFound()
    {
        await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = GetUserId()!.Value
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
            UserId = GetUserId()!.Value,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var command = new ReplyCommentCommand
        {
            CommentId = Guid.NewGuid(),
            Content = "This is a reply."
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMENT_NOT_FOUND);
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
        await AddAsync(new QuestionSetUser
        {
            UserId = ownerId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var parentComment = new Comment
        {
            Id = Guid.NewGuid(),
            QuestionId = question.Id,
            Content = "Parent comment",
            IsDeleted = false
        };
        await AddAsync(parentComment);

        await RunAsDefaultUserAsync();
        var command = new ReplyCommentCommand
        {
            CommentId = parentComment.Id,
            Content = "This is a reply."
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


        var command = new ReplyCommentCommand
        {
            CommentId = question.Id,
            Content = "This is a reply."
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
