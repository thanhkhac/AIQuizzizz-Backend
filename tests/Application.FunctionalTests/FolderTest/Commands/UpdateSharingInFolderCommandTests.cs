using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Application.FolderTest.Dto;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Commands;

using static Testing;

public class UpdateSharingInFolderCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFolderId()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateSharingInFolderCommand
        {
            FolderId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }



    [Test]
    public async Task ShouldRequireValidSharingUserIdInModels()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder" };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var command = new UpdateSharingInFolderCommand
        {
            FolderId = folder.Id,
            SharingModels = new List<UpsertSharingModelDto>
            {
                new() { SharingUserId = Guid.Empty, ShareMode = FolderShareMode.Editable.ToString() } // Invalid: Empty UserId
            }
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidShareModeInModels()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder" };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var userToShare = await RunAsUserAsync("share@local", "Share1234!", []);

        var command = new UpdateSharingInFolderCommand
        {
            FolderId = folder.Id,
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
    public async Task ShouldThrowErrorWhenFolderNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateSharingInFolderCommand
        {
            FolderId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.FOLDER_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionInFolder()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var folder = new Folder { Name = "Test Folder", CreatedBy = ownerId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = ownerId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without permission

        var command = new UpdateSharingInFolderCommand
        {
            FolderId = folder.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER);
    }

    [Test]
    public async Task ShouldUpdateSharingInFolderSuccessfully()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var folder = new Folder { Name = "Test Folder", CreatedBy = ownerId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = ownerId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var user1 = await RunAsUserAsync("user1@local", "User1234!", []);
        var user2 = await RunAsUserAsync("user2@local", "User1234!", []);
        var user3 = await RunAsUserAsync("user3@local", "User1234!", []);

        // Add initial sharing
        await AddAsync(new FolderUser { UserId = user1, FolderId = folder.Id, ShareMode = FolderShareMode.ViewOnly });
        await AddAsync(new FolderUser { UserId = user2, FolderId = folder.Id, ShareMode = FolderShareMode.Editable });

        var command = new UpdateSharingInFolderCommand
        {
            FolderId = folder.Id,
            SharingModels = new List<UpsertSharingModelDto>
            {
                new() { SharingUserId = user1, ShareMode = FolderShareMode.Editable.ToString() }, // Update user1 to Editable
                new() { SharingUserId = user3, ShareMode = FolderShareMode.ViewOnly.ToString() }  // Add user3 as ViewOnly
            },
            DeleteUserIds = new List<Guid> { user2 } // Remove user2
        };

        var result = await SendAsync(command);

        result.Should().Be(folder.Id);

        var folderUsers = await QueryListAsync<FolderUser>(x => x.Where(fu => fu.FolderId == folder.Id));
        folderUsers.Should().NotBeNull();
        folderUsers.Should().HaveCount(3); // Owner, user1 (updated), user3 (new)

        folderUsers.Should().Contain(fu => fu.UserId == ownerId && fu.ShareMode == FolderShareMode.Owner);
        folderUsers.Should().Contain(fu => fu.UserId == user1 && fu.ShareMode == FolderShareMode.Editable);
        folderUsers.Should().Contain(fu => fu.UserId == user3 && fu.ShareMode == FolderShareMode.ViewOnly);
        folderUsers.Should().NotContain(fu => fu.UserId == user2);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new UpdateSharingInFolderCommand
        {
            FolderId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
