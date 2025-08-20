using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Commands;

using static Testing;

public class AddTestTemplateToFolderCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFolderId()
    {
        await RunAsDefaultUserAsync();

        var command = new AddTestTemplateToFolderCommand
        {
            FolderId = Guid.Empty,
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidTestTemplateId()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var command = new AddTestTemplateToFolderCommand
        {
            FolderId = folder.Id,
            TestTemplateId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenFolderNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new AddTestTemplateToFolderCommand
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

        var command = new AddTestTemplateToFolderCommand
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

        await RunAsDefaultUserAsync(); // Run as a user without permission

        var command = new AddTestTemplateToFolderCommand
        {
            FolderId = folder.Id,
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestTemplateAlreadyExistsInFolder()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = userId };
        await AddAsync(testTemplate);
        await AddAsync(new FolderTestTemplate { FolderId = folder.Id, TestTemplateId = testTemplate.Id });

        var command = new AddTestTemplateToFolderCommand
        {
            FolderId = folder.Id,
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_ALREADY_EXISTS_IN_FOLDER);
    }

    [Test]
    public async Task ShouldAddTestTemplateToFolderSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = userId };
        var testTemplateUser = new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner};
        await AddAsync(testTemplate);
        await AddAsync(testTemplateUser);

        var command = new AddTestTemplateToFolderCommand
        {
            FolderId = folder.Id,
            TestTemplateId = testTemplate.Id
        };

        var result = await SendAsync(command);

        result.Should().Be(testTemplate.Id);
        //
        // var folderTestTemplate = (await QueryListAsync<FolderTestTemplate>(x => x.Where(ft => ft.FolderId == folder.Id && ft.TestTemplateId == testTemplate.Id))).FirstOrDefault();
        // folderTestTemplate.Should().NotBeNull();
        // folderTestTemplate!.FolderId.Should().Be(folder.Id);
        // folderTestTemplate.TestTemplateId.Should().Be(testTemplate.Id);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new AddTestTemplateToFolderCommand
        {
            FolderId = Guid.NewGuid(),
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
