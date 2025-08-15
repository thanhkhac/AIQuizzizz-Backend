using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Commands;

using static Testing;

public class RemoveStudentCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        await RunAsDefaultUserAsync();

        var command = new RemoveStudentCommand
        {
            ClassId = Guid.Empty,
            UserId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidUserId()
    {
        var userId = await RunAsDefaultUserAsync();

        var classEntity = new Class { Name = "Test Class" };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var command = new RemoveStudentCommand
        {
            ClassId = classEntity.Id,
            UserId = Guid.Empty
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenClassNotFound()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new RemoveStudentCommand
        {
            ClassId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.CLASS_NOTFOUND);
    }

    [Test]
    public async Task ShouldThrowErrorWhenUserIsNotOwner()
    {
        var ownerId = await RunAsUserAsync("owner@local", "Owner1234!", []);
        var classEntity = new Class { Name = "Test Class", CreatedBy = ownerId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = ownerId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var studentId = await RunAsUserAsync("student@local", "Student1234!", []);
        await AddAsync(new ClassUser { UserId = studentId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });

        await RunAsDefaultUserAsync(); // Run as a non-owner user

        var command = new RemoveStudentCommand
        {
            ClassId = classEntity.Id,
            UserId = studentId
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION);
    }

    [Test]
    public async Task ShouldThrowErrorWhenStudentNotFoundInClass()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var command = new RemoveStudentCommand
        {
            ClassId = classEntity.Id,
            UserId = Guid.NewGuid() // Non-existent student in class
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_USER_IN_CLASS);
    }

    [Test]
    public async Task ShouldRemoveStudentSuccessfully()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = ownerId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = ownerId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var studentId = await RunAsUserAsync("student@local", "Student1234!", []);
        await AddAsync(new ClassUser { UserId = studentId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });
        await         RunAsDefaultUserAsync();
        var command = new RemoveStudentCommand
        {
            ClassId = classEntity.Id,
            UserId = studentId
        };

        var result = await SendAsync(command);

        result.Should().Be(studentId);

        var removedClassUser = (await QueryListAsync<ClassUser>(x => x.Where(cu => cu.ClassId == classEntity.Id && cu.UserId == studentId))).FirstOrDefault();
        removedClassUser.Should().BeNull();
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new RemoveStudentCommand
        {
            ClassId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
