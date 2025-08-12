using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Queries;

using static Testing;

public class GetUserPermissionInClassQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new GetUserPermissionInClassQuery
        {
            ClassId = Guid.Empty
        };

        var ex = await FluentActions.Invoking((() => SendAsync(query)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserNotInClass()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var classEntity = new Class { Name = "Test Class", CreatedBy = ownerId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = ownerId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        await RunAsDefaultUserAsync(); // Run as a user not in the class

        var query = new GetUserPermissionInClassQuery
        {
            ClassId = classEntity.Id
        };

        var ex = await FluentActions.Invoking((() => SendAsync(query)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_USER_IN_CLASS);
    }

    [Test]
    public async Task ShouldReturnUserPermissionInClass()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var query = new GetUserPermissionInClassQuery
        {
            ClassId = classEntity.Id
        };

        var result = await SendAsync(query);

        result.Should().Be(ClassShareMode.Owner.ToString());

        // Test with Student role
        var studentId = await RunAsUserAsync("student@local", "Student1234!", []);
        await AddAsync(new ClassUser { UserId = studentId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });

        await RunAsUserAsync("student@local", "Student1234!", []);
        var studentQuery = new GetUserPermissionInClassQuery
        {
            ClassId = classEntity.Id
        };
        var studentResult = await SendAsync(studentQuery);
        studentResult.Should().Be(ClassShareMode.Student.ToString());
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new GetUserPermissionInClassQuery
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(query)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
