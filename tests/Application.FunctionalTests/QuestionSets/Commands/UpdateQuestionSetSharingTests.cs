using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.QuestionSets;
using CleanArchitectureBase.Application.QuestionSets.Commands;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.QuestionSets.Commands;

using static Testing;

public class UpdateQuestionSetSharingTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldUpdateVisibilityModeSuccessfully()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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

        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            VisibilityMode = "Private"
        };

        var resultId = await SendAsync(command);

        resultId.Should().Be(questionSet.Id);
        var updatedQuestionSet = await FindAsync<QuestionSet>(questionSet.Id);
        updatedQuestionSet.Should().NotBeNull();
        updatedQuestionSet!.VisibilityMode.Should().Be(QuestionSetVisibilityMode.Private);
    }

    //normal
    [Test]
    public async Task ShouldAddSharingModelsSuccessfully()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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

        await RunAsDefaultUserAsync();

        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            VisibilityMode = "Public",
            SharingModels = new List<UpsertSharingModelDto>
            {
                new()
                {
                    SharingUserId = user1Id,
                    ShareMode = "Editable"
                },
                new()
                {
                    SharingUserId = user2Id,
                    ShareMode = "ViewOnly"
                }
            }
        };

        var resultId = await SendAsync(command);

        resultId.Should().Be(questionSet.Id);
        var sharingUsers = await QueryListAsync<QuestionSetUser>(qsu => qsu.Where(x => x.QuestionSetId == questionSet.Id && x.UserId != ownerId));
        sharingUsers.Should().HaveCount(2);
        sharingUsers.Should().Contain(x => x.UserId == user1Id && x.ShareMode == QuestionSetUserShareMode.Editable);
        sharingUsers.Should().Contain(x => x.UserId == user2Id && x.ShareMode == QuestionSetUserShareMode.ViewOnly);
    }

    //normal
    [Test]
    public async Task ShouldUpdateExistingSharingModelsSuccessfully()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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
        await AddAsync(new QuestionSetUser
        {
            UserId = user1Id,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        await RunAsDefaultUserAsync();
        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            SharingModels = new List<UpsertSharingModelDto>
            {
                new()
                {
                    SharingUserId = user1Id,
                    ShareMode = "Editable"
                }
            }
        };

        var resultId = await SendAsync(command);

        resultId.Should().Be(questionSet.Id);
        var sharingUser = await FindAsync<QuestionSetUser>(user1Id, questionSet.Id);
        sharingUser.Should().NotBeNull();
        sharingUser!.ShareMode.Should().Be(QuestionSetUserShareMode.Editable);
    }

    //normal
    [Test]
    public async Task ShouldDeleteSharingModelsSuccessfully()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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
        await AddAsync(new QuestionSetUser
        {
            UserId = user1Id,
            QuestionSetId = questionSet.Id,
            ShareMode = QuestionSetUserShareMode.ViewOnly
        });

        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            DeleteUserIds = new List<Guid>
            {
                user1Id
            }
        };

        await RunAsDefaultUserAsync();

        var resultId = await SendAsync(command);

        resultId.Should().Be(questionSet.Id);
        var sharingUsers = await QueryListAsync<QuestionSetUser>(qsu => qsu.Where(x => x.QuestionSetId == questionSet.Id && x.UserId != ownerId));
        sharingUsers.Should().BeEmpty();
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenQuestionSetNotFound()
    {
        await RunAsDefaultUserAsync();
        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = Guid.NewGuid(),
            VisibilityMode = "Private"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.QUESTION_SET_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserIsNotOwner()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var questionSet = new QuestionSet
        {
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

        await RunAsDefaultUserAsync();
        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            VisibilityMode = "Private"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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

        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            VisibilityMode = "Private"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }


    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenVisibilityModeIsInvalid()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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

        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            VisibilityMode = "InvalidMode"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }


    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenShareModeIsInvalid()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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

        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            SharingModels = new List<UpsertSharingModelDto>
            {
                new()
                {
                    SharingUserId = user1Id,
                    ShareMode = "InvalidShareMode"
                }
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenSharingUserDoesNotExist()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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

        var nonExistentUserId = Guid.NewGuid();
        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            SharingModels = new List<UpsertSharingModelDto>
            {
                new()
                {
                    SharingUserId = nonExistentUserId,
                    ShareMode = "Editable"
                }
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOTFOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldNotAllowOwnerToBeRemovedViaDeleteUserIds()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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

        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            DeleteUserIds = new List<Guid>
            {
                ownerId
            }
        };

        await SendAsync(command);
        var ownerSharing = await FindAsync<QuestionSetUser>(ownerId, questionSet.Id);
        ownerSharing.Should().NotBeNull();
        ownerSharing!.ShareMode.Should().Be(QuestionSetUserShareMode.Owner);
    }

    //abnormal
    [Test]
    public async Task ShouldNotAllowOwnerShareModeToBeChangedViaSharingModels()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var questionSet = new QuestionSet
        {
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

        var command = new UpdateQuestionSetSharingCommand
        {
            QuestionSetId = questionSet.Id,
            SharingModels = new List<UpsertSharingModelDto>
            {
                new()
                {
                    SharingUserId = ownerId,
                    ShareMode = "ViewOnly"
                }
            }
        };

        await SendAsync(command);
        var ownerSharing = await FindAsync<QuestionSetUser>(ownerId, questionSet.Id);
        ownerSharing.Should().NotBeNull();
        ownerSharing!.ShareMode.Should().Be(QuestionSetUserShareMode.Owner);
    }
}
