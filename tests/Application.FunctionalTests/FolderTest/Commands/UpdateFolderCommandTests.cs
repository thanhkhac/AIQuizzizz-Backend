using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Commands;

using static Testing;

public class UpdateFolderCommandTests : BaseTestFixture
{

    [Test]
    public async Task ShouldRequireName()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var command = new UpdateFolderCommand
        {
            FolderId = folder.Id,
            Name = ""
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenFolderNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateFolderCommand
        {
            FolderId = Guid.NewGuid(),
            Name = "Updated Folder Name"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.FOLDER_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionToEditFolder()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var folder = new Folder { Name = "Test Folder", CreatedBy = ownerId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = ownerId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without edit permission

        var command = new UpdateFolderCommand
        {
            FolderId = folder.Id,
            Name = "Updated Folder Name"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER);
    }



    [Test]
    public async Task ShouldUpdateFolderSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var folder = new Folder { Name = "Original Name", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var command = new UpdateFolderCommand
        {
            FolderId = folder.Id,
            Name = "Updated Name"
        };

        var result = await SendAsync(command);

        result.Should().Be(folder.Id);

        var updatedFolder = (await QueryListAsync<Folder>(x => x.Where(f => f.Id == folder.Id))).FirstOrDefault();
        updatedFolder.Should().NotBeNull();
        updatedFolder!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new UpdateFolderCommand
        {
            FolderId = Guid.NewGuid(),
            Name = "Test"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
