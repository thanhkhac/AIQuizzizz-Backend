using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Commands;

using static Testing;

public class UpdateSharingInTestTemplateCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTestTemplateId()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateSharingInTestTemplateCommand
        {
            TestTemplateId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidSharingModels()
    {
        var userId = await RunAsDefaultUserAsync();


        var testTemplate = new TestTemplate { Name = "Test Template" };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var command = new UpdateSharingInTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            SharingModels = null // Invalid: Should not be null
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidShareModeInModels()
    {
        var userId = await RunAsDefaultUserAsync();

        var testTemplate = new TestTemplate { Name = "Test Template" };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var userToShare = await RunAsUserAsync("share@local", "Share1234!", []);

        var command = new UpdateSharingInTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            SharingModels = new List<UpsertSharingModelDto>
            {
                new() { SharingUserId = userToShare, ShareMode = "InvalidMode" } // Invalid: Invalid ShareMode
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestTemplateNotFound()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new UpdateSharingInTestTemplateCommand
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionToUpdateSharing()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without permission

        var command = new UpdateSharingInTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE);
    }

    [Test]
    public async Task ShouldUpdateSharingInTestTemplateSuccessfully()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var user1 = await RunAsUserAsync("user1@local", "User1234!", []);
        var user2 = await RunAsUserAsync("user2@local", "User1234!", []);
        var user3 = await RunAsUserAsync("user3@local", "User1234!", []);

        // Add initial sharing
        await AddAsync(new TestTemplateUser { UserId = user1, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.ViewOnly });
        await AddAsync(new TestTemplateUser { UserId = user2, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Editable });

        var command = new UpdateSharingInTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id,
            SharingModels = new List<UpsertSharingModelDto>
            {
                new() { SharingUserId = user1, ShareMode = TestTemplateUserShareMode.Editable.ToString() }, // Update user1 to Editable
                new() { SharingUserId = user3, ShareMode = TestTemplateUserShareMode.ViewOnly.ToString() }  // Add user3 as ViewOnly
            },
            DeleteUserIds = new List<Guid> { user2 } // Remove user2
        };

        var result = await SendAsync(command);

        result.Should().Be(testTemplate.Id);

        var testTemplateUsers = await QueryListAsync<TestTemplateUser>(x => x.Where(ttu => ttu.TestTemplateId == testTemplate.Id));
        testTemplateUsers.Should().NotBeNull();
        testTemplateUsers.Should().HaveCount(3); // Owner, user1 (updated), user3 (new)

        testTemplateUsers.Should().Contain(ttu => ttu.UserId == ownerId && ttu.ShareMode == TestTemplateUserShareMode.Owner);
        testTemplateUsers.Should().Contain(ttu => ttu.UserId == user1 && ttu.ShareMode == TestTemplateUserShareMode.Editable);
        testTemplateUsers.Should().Contain(ttu => ttu.UserId == user3 && ttu.ShareMode == TestTemplateUserShareMode.ViewOnly);
        testTemplateUsers.Should().NotContain(ttu => ttu.UserId == user2);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new UpdateSharingInTestTemplateCommand
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
