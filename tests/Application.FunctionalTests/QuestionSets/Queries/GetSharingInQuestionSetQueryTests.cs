using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.QuestionSets.Queries;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Queries;

using static Testing;

public class GetSharingInQuestionSetQueryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReturnSharingInfo_WhenUserIsOwner()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            Description = "Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = ownerId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        var user1Id = await RunAsUserAsync("user1@local", "User1234!", []);
        var user2Id = await RunAsUserAsync("user2@local", "User1234!", []);
        await AddAsync(new QuestionSetUser
        {
            UserId = user1Id,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Editable
        });
        await AddAsync(new QuestionSetUser
        {
            UserId = user2Id,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        await RunAsDefaultUserAsync();

        var query = new GetSharingInQuestionSetQuery
        {
            QuestionSetId = questionSet.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Id.Should().Be(questionSet.Id);
        result.VisibilityMode.Should().Be(questionSet.VisibilityMode.ToString());
        result.SharingModel.Should().HaveCount(3);
        result.SharingModel[0].ShareMode.Should().Be(QuestionSetUserShareMode.Owner.ToString());
        result.SharingModel[0].UserId.Should().Be(ownerId);

        result.SharingModel[1].ShareMode.Should().Be(QuestionSetUserShareMode.Editable.ToString());
        result.SharingModel[1].UserId.Should().Be(user1Id);

        result.SharingModel[2].ShareMode.Should().Be(QuestionSetUserShareMode.ViewOnly.ToString());
        result.SharingModel[2].UserId.Should().Be(user2Id);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var query = new GetSharingInQuestionSetQuery
        {
            QuestionSetId = Guid.NewGuid()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserIsNotOwner()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            Description = "Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Private
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = ownerId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        await RunAsDefaultUserAsync();
        var query = new GetSharingInQuestionSetQuery
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_ACCESS_TO_QUESTION_SET);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
            Id = Guid.NewGuid(),
            Name = "Test Question Set",
            Description = "Description",
            CreatedBy = ownerId,
            VisibilityMode = QuestionSetVisibilityMode.Public
        };
        await AddAsync(questionSet);
        await AddAsync(new QuestionSetUser
        {
            UserId = ownerId,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.Owner
        });

        Logout();

        var query = new GetSharingInQuestionSetQuery
        {
            QuestionSetId = questionSet.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
