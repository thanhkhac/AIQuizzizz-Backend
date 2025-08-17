using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.TestTemplates;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.TestTemplates.Queries;

using static Testing;

public class GetTestTemplateDetailQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTestTemplateId()
    {
        await RunAsDefaultUserAsync();

        var query = new GetTestTemplateDetailQuery
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

        var query = new GetTestTemplateDetailQuery
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.TEST_TEMPLATE_NOT_FOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotHavePermissionToViewTestTemplate()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var testTemplate = new TestTemplate { Name = "Test Template", CreatedBy = ownerId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = ownerId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user without permission

        var query = new GetTestTemplateDetailQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE);
    }

    [Test]
    public async Task ShouldReturnTestTemplateDetail()
    {
        var userId = await RunAsDefaultUserAsync();
        var testTemplate = new TestTemplate { Name = "Test Template", Description = "Test Description", CreatedBy = userId };
        await AddAsync(testTemplate);
        await AddAsync(new TestTemplateUser { UserId = userId, TestTemplateId = testTemplate.Id, ShareMode = TestTemplateUserShareMode.Owner });

        var question1 = new Question { QuestionText = "Q1", Type = QuestionType.ShortText, Score = 1, DataJson = "{}" };
        var question2 = new Question { QuestionText = "Q2", Type = QuestionType.ShortText, Score = 1, DataJson = "{}" };
        await AddAsync(question1);
        await AddAsync(question2);

        await AddAsync(new TestTemplateQuestion { TestTemplateId = testTemplate.Id, QuestionId = question1.Id });
        await AddAsync(new TestTemplateQuestion { TestTemplateId = testTemplate.Id, QuestionId = question2.Id });

        var query = new GetTestTemplateDetailQuery
        {
            TestTemplateId = testTemplate.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.TestTemplateId.Should().Be(testTemplate.Id);
        result.Name.Should().Be(testTemplate.Name);
        result.Description.Should().Be(testTemplate.Description);
        result.QuestionCount.Should().Be(2);
        result.Questions.Should().HaveCount(2);
        result.Questions.Should().Contain(q => q.QuestionText == "Q1");
        result.Questions.Should().Contain(q => q.QuestionText == "Q2");
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new GetTestTemplateDetailQuery
        {
            TestTemplateId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
