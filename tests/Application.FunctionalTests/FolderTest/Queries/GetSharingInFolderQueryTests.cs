using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Queries;

using static Testing;

public class GetSharingInFolderQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFolderId()
    {
        await RunAsDefaultUserAsync();

        var query = new GetSharingInFolderQuery
        {
            FolderId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenFolderNotFound()
    {
        await RunAsDefaultUserAsync();

        var query = new GetSharingInFolderQuery
        {
            FolderId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
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

        var query = new GetSharingInFolderQuery
        {
            FolderId = folder.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER);
    }

    [Test]
    public async Task ShouldReturnSharingInFolder()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var folder = new Folder { Name = "Test Folder", CreatedBy = ownerId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = ownerId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var editorId = await RunAsUserAsync("editor@local", "Editor1234!", []);
        await AddAsync(new FolderUser { UserId = editorId, FolderId = folder.Id, ShareMode = FolderShareMode.Editable });

        var viewerId = await RunAsUserAsync("viewer@local", "Viewer1234!", []);
        await AddAsync(new FolderUser { UserId = viewerId, FolderId = folder.Id, ShareMode = FolderShareMode.ViewOnly });
    
        await RunAsDefaultUserAsync();
        var query = new GetSharingInFolderQuery
        {
            FolderId = folder.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Id.Should().Be(folder.Id);
        result.SharingModel.Should().HaveCount(3);

        result.SharingModel.Should().Contain(s => s.UserId == ownerId && s.ShareMode == FolderShareMode.Owner.ToString());
        result.SharingModel.Should().Contain(s => s.UserId == editorId && s.ShareMode == FolderShareMode.Editable.ToString());
        result.SharingModel.Should().Contain(s => s.UserId == viewerId && s.ShareMode == FolderShareMode.ViewOnly.ToString());

        // Check order
        result.SharingModel[0].ShareMode.Should().Be(FolderShareMode.Owner.ToString());
        result.SharingModel[1].ShareMode.Should().Be(FolderShareMode.Editable.ToString());
        result.SharingModel[2].ShareMode.Should().Be(FolderShareMode.ViewOnly.ToString());
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new GetSharingInFolderQuery
        {
            FolderId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
