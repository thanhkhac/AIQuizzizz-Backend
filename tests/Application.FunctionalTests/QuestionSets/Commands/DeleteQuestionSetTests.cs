using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Commands;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Commands;

using static Testing;

public class DeleteQuestionSetTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldDeleteQuestionSet_WhenUserIsOwner()
    {
        var userId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Name = "Test Question Set",
            Description = "Description",
            CreatedBy = userId
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = userId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var command = new DeleteQuestionSetCommand
        {
            QuestionSetId = questionSet.Id
        };

        await SendAsync(command);

        var deletedQuestionSet = await FindAsync<QuestionSet>(questionSet.Id);
        deletedQuestionSet.Should().BeNull();
    }

    //normal
    [Test]
    public async Task ShouldThrowError_WhenUserHasEditableShareMode()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Name = "Test Question Set",
            Description = "Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        var editorId = await RunAsUserAsync("editor@local", "Editor1234!", []);
        await AddAsync(new QuestionSetUser
        {
            UserId = editorId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Editable
        });

        var command = new DeleteQuestionSetCommand
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var command = new DeleteQuestionSetCommand
        {
            QuestionSetId = Guid.NewGuid()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserHasViewOnlyShareMode()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Name = "Test Question Set",
            Description = "Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        var viewerId = await RunAsUserAsync("viewer@local", "Viewer1234!", []);
        await AddAsync(new QuestionSetUser
        {
            UserId = viewerId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        var command = new DeleteQuestionSetCommand
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserHasNoShareMode()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Name = "Test Question Set",
            Description = "Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);

        await RunAsDefaultUserAsync();
        var command = new DeleteQuestionSetCommand
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var questionSet = new QuestionSet
        {
            Name = "Test Question Set",
            Description = "Description",
            CreatedBy = Guid.NewGuid()
        };
        await AddAsync(questionSet);


        var command = new DeleteQuestionSetCommand
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetIdIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var command = new DeleteQuestionSetCommand
        {
            QuestionSetId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
}
