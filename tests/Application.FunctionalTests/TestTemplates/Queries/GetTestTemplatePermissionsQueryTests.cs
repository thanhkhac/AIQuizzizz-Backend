using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Queries;

using static Testing;

public class GetTestTemplatePermissionsQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTestTemplateId()
    {
        await RunAsDefaultUserAsync();

        var query = new GetTestTemplatePermissionsQuery
        {
            TestTemplateId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestTemplateNotFound()
    {
        await RunAsDefaultUserAsync();

        var query = new GetTestTemplatePermissionsQuery
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_NOT_FOUND);
    }

    [Test]
    public async Task ShouldReturnCorrectPermissionsForOwner()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var query = new GetTestTemplatePermissionsQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.CanEdit.Should().BeTrue();
        result.CanDelete.Should().BeTrue();
    }

    [Test]
    public async Task ShouldReturnCorrectPermissionsForEditor()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var editorId = await RunAsDefaultUserAsync();
        await AddAsync(new TestTemplateUser { UserId = editorId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Editable });

        var query = new GetTestTemplatePermissionsQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.CanEdit.Should().BeTrue();
        result.CanDelete.Should().BeFalse();
    }

    [Test]
    public async Task ShouldReturnCorrectPermissionsForViewer()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var viewerId = await RunAsDefaultUserAsync();
        await AddAsync(new TestTemplateUser { UserId = viewerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.ViewOnly });

        var query = new GetTestTemplatePermissionsQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.CanEdit.Should().BeFalse();
        result.CanDelete.Should().BeFalse();
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionInTestTemplate()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        await RunAsUserAsync("unauthorized@local", "Unauthorized1234!", []); // Run as a user not in the test template

        var query = new GetTestTemplatePermissionsQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE);
    }

    [Test]
    public async Task ShouldReturnFalseForPermissionsWhenNotLoggedIn()
    {
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = Guid.NewGuid() };
        await AddAsync(testTemplate);

        var query = new GetTestTemplatePermissionsQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
