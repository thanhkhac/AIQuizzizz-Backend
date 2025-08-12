using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.FolderTest;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.FolderTest.Queries;

using static Testing;

public class SearchTestTemplateInFolderQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidFolderId()
    {
        await RunAsDefaultUserAsync();

        var query = new SearchTestTemplateInFolderQuery
        {
            FolderId = null
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionInFolder()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var folder = new Folder { Name = "Test Folder", CreatedBy = ownerId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = ownerId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without permission

        var query = new SearchTestTemplateInFolderQuery
        {
            FolderId = folder.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_FOLDER);
    }

    [Test]
    public async Task ShouldReturnTestTemplatesInFolder()
    {
        var userId = await RunAsDefaultUserAsync();
        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var tt1 = new TestTemplate { Name = "TestTemplate 1", CreatedBy = userId };
        var tt2 = new TestTemplate { Name = "TestTemplate 2", CreatedBy = userId };
        var tt3 = new TestTemplate { Name = "TestTemplate 3", CreatedBy = userId };
        await AddAsync(tt1);
        await AddAsync(tt2);
        await AddAsync(tt3);

        await AddAsync(new FolderTestTemplate { FolderId = folder.Id, TestTemplateId = tt1.Id });
        await AddAsync(new FolderTestTemplate { FolderId = folder.Id, TestTemplateId = tt3.Id });

        var query = new SearchTestTemplateInFolderQuery
        {
            FolderId = folder.Id,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.FolderName.Should().Be(folder.Name);
        result.TestTemplates.Items.Should().HaveCount(2);
        result.TestTemplates.Items.Should().Contain(t => t.Name == "TestTemplate 1");
        result.TestTemplates.Items.Should().Contain(t => t.Name == "TestTemplate 3");
        result.TestTemplates.Items.Should().NotContain(t => t.Name == "TestTemplate 2");
    }

    [Test]
    public async Task ShouldFilterTestTemplatesByName()
    {
        var userId = await RunAsDefaultUserAsync();
        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        var tt1 = new TestTemplate { Name = "Math Test Template", CreatedBy = userId };
        var tt2 = new TestTemplate { Name = "Science Test Template", CreatedBy = userId };
        await AddAsync(tt1);
        await AddAsync(tt2);

        await AddAsync(new FolderTestTemplate { FolderId = folder.Id, TestTemplateId = tt1.Id });
        await AddAsync(new FolderTestTemplate { FolderId = folder.Id, TestTemplateId = tt2.Id });

        var query = new SearchTestTemplateInFolderQuery
        {
            FolderId = folder.Id,
            TestTemplateName = "math",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.TestTemplates.Items.Should().HaveCount(1);
        result.TestTemplates.Items.First().Name.Should().Be("Math Test Template");
    }

    [Test]
    public async Task ShouldReturnPaginatedResults()
    {
        var userId = await RunAsDefaultUserAsync();
        var folder = new Folder { Name = "Test Folder", CreatedBy = userId };
        await AddAsync(folder);
        await AddAsync(new FolderUser { UserId = userId, FolderId = folder.Id, ShareMode = FolderShareMode.Owner });

        for (int i = 0; i < 15; i++)
        {
            var tt = new TestTemplate { Name = $"TestTemplate {i}", CreatedBy = userId };
            await AddAsync(tt);
            await AddAsync(new FolderTestTemplate { FolderId = folder.Id, TestTemplateId = tt.Id });
        }

        var query = new SearchTestTemplateInFolderQuery
        {
            FolderId = folder.Id,
            PageNumber = 2,
            PageSize = 5
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.TestTemplates.Items.Should().HaveCount(5);
        result.TestTemplates.TotalCount.Should().Be(15);
        result.TestTemplates.PageNumber.Should().Be(2);
        result.TestTemplates.TotalPages.Should().Be(3);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new SearchTestTemplateInFolderQuery
        {
            FolderId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
