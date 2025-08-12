using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Queries;

using static Testing;

public class GetClassByIdQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        var userId = await RunAsDefaultUserAsync();

        var query = new GetClassByIdQuery
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

        var query = new GetClassByIdQuery
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.CLASS_NOTFOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenClassIsDeleted()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", IsDeleted = true };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var query = new GetClassByIdQuery
        {
            ClassId = classEntity.Id
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

        var query = new GetClassByIdQuery
        {
            ClassId = classEntity.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_USER_IN_CLASS);
    }

    [Test]
    public async Task ShouldGetClassByIdSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", Topic = "Test Topic", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var query = new GetClassByIdQuery
        {
            ClassId = classEntity.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.ClassId.Should().Be(classEntity.Id);
        result.Name.Should().Be(classEntity.Name);
        result.Topic.Should().Be(classEntity.Topic);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new GetClassByIdQuery
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
