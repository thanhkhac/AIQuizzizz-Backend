using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Queries;

using static Testing;

public class SearchTestTemplateQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidShareMode()
    {
        await RunAsDefaultUserAsync();

        var query = new SearchTestTemplateQuery
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
    public async Task ShouldReturnTestTemplatesForCurrentUser()
    {
        var userId = await RunAsDefaultUserAsync();

        var tt1 = new TestTemplate { Name = "TestTemplate A", CreatedBy = userId };
        var tt2 = new TestTemplate { Name = "TestTemplate B", CreatedBy = Guid.NewGuid() }; // Another user's test template
        var tt3 = new TestTemplate { Name = "TestTemplate C", CreatedBy = userId };
        await AddAsync(tt1);
        await AddAsync(tt2);
        await AddAsync(tt3);
        
        var userId2 = await RunAsDefaultUserAsync(1);

        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = tt1.Id, ShareMode = TestTemplateUserShareMode.Owner });
        await AddAsync(new TestTemplateUser { UserId = userId2, TestTemplateId = tt2.Id, ShareMode = TestTemplateUserShareMode.Owner });
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = tt3.Id, ShareMode = TestTemplateUserShareMode.ViewOnly });

        await RunAsDefaultUserAsync();
        var query = new SearchTestTemplateQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCountGreaterThan(0); 
    }

    [Test]
    public async Task ShouldFilterTestTemplatesByName()
    {
        var userId = await RunAsDefaultUserAsync();

        var tt1 = new TestTemplate { Name = "Math Test Template", CreatedBy = userId };
        var tt2 = new TestTemplate { Name = "Science Test Template", CreatedBy = userId };
        var tt3 = new TestTemplate { Name = "History Test Template", CreatedBy = userId };
        await AddAsync(tt1);
        await AddAsync(tt2);
        await AddAsync(tt3);

        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = tt1.Id, ShareMode = TestTemplateUserShareMode.Owner });
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = tt2.Id, ShareMode = TestTemplateUserShareMode.Owner });
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = tt3.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var query = new SearchTestTemplateQuery
        {
            PageNumber = 1,
            PageSize = 10,
            TestTemplateName = "math"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Math Test Template");
    }

    [Test]
    public async Task ShouldFilterTestTemplatesByShareMode()
    {
        var userId = await RunAsDefaultUserAsync();

        var tt1 = new TestTemplate { Name = "Owner Test Template" };
        var tt2 = new TestTemplate { Name = "Editable Test Template" };
        var tt3 = new TestTemplate { Name = "ViewOnly Test Template" };
        await AddAsync(tt1);
        await AddAsync(tt2);
        await AddAsync(tt3);

        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = tt1.Id, ShareMode = TestTemplateUserShareMode.Owner });
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = tt2.Id, ShareMode = TestTemplateUserShareMode.Editable });
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = tt3.Id, ShareMode = TestTemplateUserShareMode.ViewOnly });

        var query = new SearchTestTemplateQuery
        {
            PageNumber = 1,
            PageSize = 10,
            ShareMode = "Owner"
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCountGreaterThan(0);
    }

    [Test]
    public async Task ShouldReturnPaginatedResults()
    {
        var userId = await RunAsDefaultUserAsync();

        for (int i = 0; i < 15; i++)
        {
            var testTemplate = new TestTemplate { Name = $"TestTemplate {i}", CreatedBy = userId };
            await AddAsync(testTemplate);
            await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });
        }

        var query = new SearchTestTemplateQuery
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
        var query = new SearchTestTemplateQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
