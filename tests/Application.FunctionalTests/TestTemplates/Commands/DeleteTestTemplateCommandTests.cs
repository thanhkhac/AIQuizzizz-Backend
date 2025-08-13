using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Commands;

using static Testing;

public class DeleteTestTemplateCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTestTemplateId()
    {
        await RunAsDefaultUserAsync();

        var command = new DeleteTestTemplateCommand
        {
            TestTemplateId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenTestTemplateNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new DeleteTestTemplateCommand
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionToDeleteTestTemplate()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without delete permission

        var command = new DeleteTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE);
    }

    [Test]
    public async Task ShouldSoftDeleteTestTemplateSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();

        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = userId, IsDeleted = false };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var command = new DeleteTestTemplateCommand
        {
            TestTemplateId = testTemplate.Id
        };

        var result = await SendAsync(command);

        result.Should().Be(testTemplate.Id);

        var deletedTestTemplate = (await QueryListAsync<TestTemplate>(x => x.Where(tt => tt.Id == testTemplate.Id))).FirstOrDefault();
        deletedTestTemplate.Should().NotBeNull();
        deletedTestTemplate!.IsDeleted.Should().BeTrue();
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new DeleteTestTemplateCommand
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
