using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Queries;

using static Testing;

public class GetSharingInTestTemplateQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTestTemplateId()
    {
        await RunAsDefaultUserAsync();

        var query = new GetSharingInTestTemplateQuery
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

        var query = new GetSharingInTestTemplateQuery
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionToViewSharing()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without permission

        var query = new GetSharingInTestTemplateQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE);
    }

    [Test]
    public async Task ShouldReturnSharingInTestTemplate()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var editorId = await RunAsUserAsync("editor@local", "Editor1234!", []);
        await AddAsync(new TestTemplateUser { UserId = editorId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Editable });

        var viewerId = await RunAsUserAsync("viewer@local", "Viewer1234!", []);
        await AddAsync(new TestTemplateUser { UserId = viewerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.ViewOnly });

        var query = new GetSharingInTestTemplateQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Id.Should().Be(testTemplate.Id);
        result.SharingModel.Should().HaveCount(3);

        result.SharingModel.Should().Contain(s => s.UserId == ownerId && s.ShareMode == TestTemplateUserShareMode.Owner.ToString());
        result.SharingModel.Should().Contain(s => s.UserId == editorId && s.ShareMode == TestTemplateUserShareMode.Editable.ToString());
        result.SharingModel.Should().Contain(s => s.UserId == viewerId && s.ShareMode == TestTemplateUserShareMode.ViewOnly.ToString());

        // Check order
        result.SharingModel[0].ShareMode.Should().Be(TestTemplateUserShareMode.Owner.ToString());
        result.SharingModel[1].ShareMode.Should().Be(TestTemplateUserShareMode.Editable.ToString());
        result.SharingModel[2].ShareMode.Should().Be(TestTemplateUserShareMode.ViewOnly.ToString());
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new GetSharingInTestTemplateQuery
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
