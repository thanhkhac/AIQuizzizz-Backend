using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Queries;

using static Testing;

public class SearchQuestionSetQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchQuestionSetQuery
        {
            ClassId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenClassNotFound()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchQuestionSetQuery
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.CLASS_NOTFOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotInClass()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var classEntity = new Class { Name = "Test Class", CreatedBy = ownerId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = ownerId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user not in the class

        var query = new SearchQuestionSetQuery
        {
            ClassId = classEntity.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_USER_IN_CLASS);
    }

    [Test]
    public async Task ShouldReturnQuestionSetsInClass()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var qs1 = new QuestionSet { Name = "QuestionSet 1", CreatedBy = userId, VisibilityMode = QuestionSetVisibilityMode.Public };
        var qs2 = new QuestionSet { Name = "QuestionSet 2", CreatedBy = userId, VisibilityMode = QuestionSetVisibilityMode.Public };
        var qs3 = new QuestionSet { Name = "QuestionSet 3", CreatedBy = userId, VisibilityMode = QuestionSetVisibilityMode.Public };
        await AddAsync(qs1);
        await AddAsync(qs2);
        await AddAsync(qs3);

        await AddAsync(new ClassQuestionSet { ClassId = classEntity.Id, QuestionSetId = qs1.Id });
        await AddAsync(new ClassQuestionSet { ClassId = classEntity.Id, QuestionSetId = qs3.Id });

        var query = new SearchQuestionSetQuery
        {
            ClassId = classEntity.Id,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(q => q.Name == "QuestionSet 1");
        result.Items.Should().Contain(q => q.Name == "QuestionSet 3");
        result.Items.Should().NotContain(q => q.Name == "QuestionSet 2");
    }

    [Test]
    public async Task ShouldFilterQuestionSetsByName()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var qs1 = new QuestionSet { Name = "Math Questions", CreatedBy = userId, VisibilityMode = QuestionSetVisibilityMode.Public };
        var qs2 = new QuestionSet { Name = "Science Questions", CreatedBy = userId, VisibilityMode = QuestionSetVisibilityMode.Public };
        await AddAsync(qs1);
        await AddAsync(qs2);

        await AddAsync(new ClassQuestionSet { ClassId = classEntity.Id, QuestionSetId = qs1.Id });
        await AddAsync(new ClassQuestionSet { ClassId = classEntity.Id, QuestionSetId = qs2.Id });

        var query = new SearchQuestionSetQuery
        {
            ClassId = classEntity.Id,
            Name = "math",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Math Questions");
    }

    [Test]
    public async Task ShouldReturnPaginatedResults()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        for (int i = 0; i < 15; i++)
        {
            var qs = new QuestionSet { Name = $"QuestionSet {i}", CreatedBy = userId, VisibilityMode = QuestionSetVisibilityMode.Public };
            await AddAsync(qs);
            await AddAsync(new ClassQuestionSet { ClassId = classEntity.Id, QuestionSetId = qs.Id });
        }

        var query = new SearchQuestionSetQuery
        {
            ClassId = classEntity.Id,
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
        var query = new SearchQuestionSetQuery
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
