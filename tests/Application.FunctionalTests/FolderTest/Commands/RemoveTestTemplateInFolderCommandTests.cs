using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Commands;

using static Testing;

public class RemoveTestTemplateInFolderCommandTests : BaseTestFixture
{


    [Test]
    public async Task ShouldThrowErrorWhenFolderNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new RemoveTestTemplateInFolderCommand
        {
            FolderId = Guid.NewGuid(),
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.FOLDER_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestTemplateNotFound()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var command = new RemoveTestTemplateInFolderCommand
        {
            FolderId = folder.Id,
            TestTemplateId = Guid.NewGuid() // Non-existent TestTemplateId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionInFolder()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var folder = new Folder { Name = "Test Folder", CreatedBy = ownerId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = ownerId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new FolderTestTemplate { FolderId = folder.Id, TestTemplateId = testTemplate.Id });

        await RunAsDefaultUserAsync(); // Run as a user without permission

        var command = new RemoveTestTemplateInFolderCommand
        {
            FolderId = folder.Id,
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestTemplateNotInFolder()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = userId };
        await AddAsync(testTemplate);

        var command = new RemoveTestTemplateInFolderCommand
        {
            FolderId = folder.Id,
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_NOT_FOUND_IN_FOLDER);
    }

    [Test]
    public async Task ShouldRemoveTestTemplateInFolderSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = userId };
        await AddAsync(testTemplate);
        await AddAsync(new FolderTestTemplate { FolderId = folder.Id, TestTemplateId = testTemplate.Id });

        var command = new RemoveTestTemplateInFolderCommand
        {
            FolderId = folder.Id,
            TestTemplateId = testTemplate.Id
        };

        var result = await SendAsync(command);

        result.Should().Be(testTemplate.Id);

        var removedFolderTestTemplate = (await QueryListAsync<FolderTestTemplate>(x => x.Where(ft => ft.FolderId == folder.Id && ft.TestTemplateId == testTemplate.Id))).FirstOrDefault();
        removedFolderTestTemplate.Should().BeNull();
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new RemoveTestTemplateInFolderCommand
        {
            FolderId = Guid.NewGuid(),
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
