using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class GetQuestionSetPermissionsQueryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReturnTrueForEditAndDelete_WhenUserIsOwner()
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

        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.CanEdit.Should().BeTrue();
        result.CanDelete.Should().BeTrue();
    }

    //normal
    [Test]
    public async Task ShouldReturnTrueForEditAndDelete_WhenUserHasEditableShareMode()
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

        var editorId = await RunAsUserAsync("editor@local", "Editor1234!", []);
        await AddAsync(new QuestionSetUser
        {
            UserId = editorId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Editable
        });

        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.CanEdit.Should().BeTrue();
        result.CanDelete.Should().BeFalse();
    }

    //normal
    [Test]
    public async Task ShouldReturnFalseForEditAndDelete_WhenUserHasViewOnlyShareMode()
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

        var viewerId = await RunAsUserAsync("viewer@local", "Viewer1234!", []);
        await AddAsync(new QuestionSetUser
        {
            UserId = viewerId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.CanEdit.Should().BeFalse();
        result.CanDelete.Should().BeFalse();
    }

    //normal
    [Test]
    public async Task ShouldReturnFalseForEditAndDelete_WhenUserHasNoShareMode()
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

        await RunAsDefaultUserAsync();
        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.CanEdit.Should().BeFalse();
        result.CanDelete.Should().BeFalse();
    }

    //normal
    [Test]
    public async Task ShouldReturnFalseForEditAndDelete_WhenUserIsNotLoggedIn()
    {
        var ownerId = Guid.NewGuid();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        await AddAsync(questionSet);


        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.CanEdit.Should().BeFalse();
        result.CanDelete.Should().BeFalse();
    }

    //abnormal
    [Test]
    public async Task ShouldReturnFalseForEditAndDelete_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = Guid.NewGuid()
        };
        var result = await SendAsync(query);
        result.Should().NotBeNull();
        result.CanEdit.Should().BeFalse();
        result.CanDelete.Should().BeFalse();
    }

    //abnormal
    [Test]
    public async Task ShouldReturnFalseForEditAndDelete_WhenQuestionSetIdIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var query = new GetQuestionSetPermissionsQuery
        {
            QuestionSetId = Guid.NewGuid()
        };

        var result = await SendAsync(query);
        result.Should().NotBeNull();
        result.CanEdit.Should().BeFalse();
        result.CanDelete.Should().BeFalse();
    }
}
