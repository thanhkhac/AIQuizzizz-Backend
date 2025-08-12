using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Queries;

using static Testing;

public class SearchStudentInClassQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchStudentInClassQuery
        {
            ClassId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidPageNumber()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchStudentInClassQuery
        {
            ClassId = Guid.NewGuid(),
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
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchStudentInClassQuery
        {
            ClassId = Guid.NewGuid(),
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
    public async Task ShouldThrowErrorWhenClassNotFound()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new SearchStudentInClassQuery
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

        var query = new SearchStudentInClassQuery
        {
            ClassId = classEntity.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_USER_IN_CLASS);
    }

    [Test]
    public async Task ShouldReturnStudentsInClass()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var student1 = await RunAsUserAsync("student1@local", "Student1234!", []);
        var student2 = await RunAsUserAsync("student2@local", "Student1234!", []);
        await AddAsync(new ClassUser { UserId = student1, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });
        await AddAsync(new ClassUser { UserId = student2, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });

        var query = new SearchStudentInClassQuery
        {
            ClassId = classEntity.Id,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(s => s.Email == "student1@local");
        result.Items.Should().Contain(s => s.Email == "student2@local");
    }

    [Test]
    public async Task ShouldFilterStudentsByKeywordAndFieldName()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var student1 = await RunAsUserAsync("alice@local", "Student1234!", []);
        var student2 = await RunAsUserAsync("bob@local", "Student1234!", []);
        await AddAsync(new ClassUser { UserId = student1, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });
        await AddAsync(new ClassUser { UserId = student2, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });

        var query = new SearchStudentInClassQuery
        {
            ClassId = classEntity.Id,
            Keyword = "ali",
            FieldName = "Email",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Email.Should().Be("alice@local");
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
            var student = await RunAsUserAsync($"student{i}@local", "Student1234!", []);
            await AddAsync(new ClassUser { UserId = student, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });
        }

        var query = new SearchStudentInClassQuery
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
        var query = new SearchStudentInClassQuery
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
