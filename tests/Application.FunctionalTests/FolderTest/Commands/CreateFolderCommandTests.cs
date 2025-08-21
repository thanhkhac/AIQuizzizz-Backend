using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Commands;

using static Testing;

public class CreateFolderCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireFolderName()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateFolderCommand
        {
            FolderName = ""
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireFolderNameNotTooLong()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateFolderCommand
        {
            FolderName = new string('a', 201)
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }
    
    [Test]
    public async Task ShouldRequireFolderNameLong()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateFolderCommand
        {
            FolderName = new string('a', 199)
        };

        var folderId = await SendAsync(command);

    }

    [Test]
    public async Task ShouldCreateFolderSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateFolderCommand
        {
            FolderName = "New Test Folder"
        };

        var folderId = await SendAsync(command);

        var newFolder = (await QueryListAsync<Folder>(x => x.Where(f => f.Id == folderId))).FirstOrDefault();
        newFolder.Should().NotBeNull();
        newFolder!.Name.Should().Be(command.FolderName);
        newFolder.CreatedBy.Should().Be(userId);

        var folderUser = (await QueryListAsync<FolderUser>(x => x.Where(fu => fu.FolderId == folderId && fu.UserId == userId))).FirstOrDefault();
        folderUser.Should().NotBeNull();
        folderUser!.ShareMode.Should().Be(FolderShareMode.Owner);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new CreateFolderCommand
        {
            FolderName = "New Test Folder"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
