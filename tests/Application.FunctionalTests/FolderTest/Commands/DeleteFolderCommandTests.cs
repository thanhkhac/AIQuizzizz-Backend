using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Commands;

using static Testing;

public class DeleteFolderCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFolderId()
    {
        await RunAsDefaultUserAsync();

        var command = new DeleteFolderCommand
        {
            FolderId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenFolderNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new DeleteFolderCommand
        {
            FolderId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.FOLDER_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionToDeleteFolder()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var folder = new Folder { Name = "Test Folder", CreatedBy = ownerId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = ownerId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without delete permission

        var command = new DeleteFolderCommand
        {
            FolderId = folder.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER);
    }

    [Test]
    public async Task ShouldSoftDeleteFolderSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder", CreatedBy = userId, IsDeleted = false };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var command = new DeleteFolderCommand
        {
            FolderId = folder.Id
        };

        var result = await SendAsync(command);

        result.Should().Be(folder.Id);

        var deletedFolder = (await QueryListAsync<Folder>(x => x.Where(f => f.Id == folder.Id))).FirstOrDefault();
        deletedFolder.Should().NotBeNull();
        deletedFolder!.IsDeleted.Should().BeTrue();
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new DeleteFolderCommand
        {
            FolderId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
