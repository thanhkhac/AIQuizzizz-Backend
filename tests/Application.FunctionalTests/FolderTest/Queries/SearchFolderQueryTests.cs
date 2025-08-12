using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Queries;

using static Testing;

public class SearchFolderQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidPageNumber()
    {
        await RunAsDefaultUserAsync();

        var query = new SearchFolderQuery
        {
            PageNumber = 0,
            PageSize = 10
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidPageSize()
    {
        await RunAsDefaultUserAsync();

        var query = new SearchFolderQuery
        {
            PageNumber = 1,
            PageSize = 0
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);

        query.PageSize = 101;
        ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidShareMode()
    {
        await RunAsDefaultUserAsync();

        var query = new SearchFolderQuery
        {
            PageNumber = 1,
            PageSize = 10,
            ShareMode = "InvalidMode"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldReturnFoldersForCurrentUser()
    {
        var userId = await RunAsDefaultUserAsync();
        var userId1 = await RunAsDefaultUserAsync(1);

        var folder1 = new Folder { Name = "Folder A", CreatedBy = userId };
        var folder2 = new Folder { Name = "Folder B", CreatedBy = userId1}; 
        var folder3 = new Folder { Name = "Folder C", CreatedBy = userId };
        
        await AddAsync(folder1);
        await AddAsync(folder2);
        await AddAsync(folder3);

        await AddAsync(new FolderUser { UserId = userId, FolderId = folder1.Id, ShareMode = FolderShareMode.Owner });
        await AddAsync(new FolderUser { UserId = userId1, FolderId = folder2.Id, ShareMode = FolderShareMode.Owner });
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder3.Id, ShareMode = FolderShareMode.ViewOnly });

        var query = new SearchFolderQuery
        {
            PageNumber = 1,
            PageSize = 10
        };
        await RunAsDefaultUserAsync();
        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2); // Folder A and Folder C
        result.Items.Should().Contain(f => f.Name == "Folder A");
        result.Items.Should().Contain(f => f.Name == "Folder C");
        result.Items.Should().NotContain(f => f.Name == "Folder B");
    }

    [Test]
    public async Task ShouldFilterFoldersByName()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder1 = new Folder { Name = "Math Folder", CreatedBy = userId };
        var folder2 = new Folder { Name = "Science Folder", CreatedBy = userId };
        var folder3 = new Folder { Name = "History Folder", CreatedBy = userId };
        await AddAsync(folder1);
        await AddAsync(folder2);
        await AddAsync(folder3);

        await AddAsync(new FolderUser { UserId = userId, FolderId = folder1.Id, ShareMode = FolderShareMode.Owner });
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder2.Id, ShareMode = FolderShareMode.Owner });
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder3.Id, ShareMode = FolderShareMode.Owner });

        var query = new SearchFolderQuery
        {
            PageNumber = 1,
            PageSize = 10,
            FolderName = "math"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Math Folder");
    }

    [Test]
    public async Task ShouldFilterFoldersByShareMode()
    {
        var userId = await RunAsDefaultUserAsync();

        var folder1 = new Folder { Name = "Owner Folder", CreatedBy = userId };
        var folder2 = new Folder { Name = "Editable Folder", CreatedBy = Guid.NewGuid() };
        var folder3 = new Folder { Name = "ViewOnly Folder", CreatedBy = Guid.NewGuid() };
        await AddAsync(folder1);
        await AddAsync(folder2);
        await AddAsync(folder3);

        await AddAsync(new FolderUser { UserId = userId, FolderId = folder1.Id, ShareMode = FolderShareMode.Owner });
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder2.Id, ShareMode = FolderShareMode.Editable });
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder3.Id, ShareMode = FolderShareMode.ViewOnly });

        var query = new SearchFolderQuery
        {
            PageNumber = 1,
            PageSize = 10,
            ShareMode = "Editable"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Editable Folder");
    }

    [Test]
    public async Task ShouldReturnPaginatedResults()
    {
        var userId = await RunAsDefaultUserAsync();

        for (int i = 0; i < 15; i++)
        {
            var folder = new Folder { Name = $"Folder {i}", CreatedBy = userId };
            await AddAsync(folder);
            await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });
        }

        var query = new SearchFolderQuery
        {
            PageNumber = 2,
            PageSize = 5
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.TotalPages.Should().Be(3);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new SearchFolderQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
