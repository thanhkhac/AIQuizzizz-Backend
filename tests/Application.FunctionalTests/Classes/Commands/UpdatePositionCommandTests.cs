using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Commands;

using static Testing;

public class UpdatePositionCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdatePositionCommand
        {
            ClassId = null,
            UserId = Guid.NewGuid(),
            Position = ClassShareMode.Teacher.ToString()
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

        var command = new UpdatePositionCommand
        {
            ClassId = classEntity.Id,
            UserId = null,
            Position = ClassShareMode.Teacher.ToString()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireValidPosition()
    {
        var userId =  await RunAsDefaultUserAsync();

        var classEntity = new Class { Name = "Test Class" };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var studentId = await RunAsUserAsync("student@local", "Student1234!", []);
        await AddAsync(new ClassUser { UserId = studentId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });

        var command = new UpdatePositionCommand
        {
            ClassId = classEntity.Id,
            UserId = studentId,
            Position = "InvalidPosition"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    // [Test]
    // public async Task ShouldThrowErrorWhenClassNotFound()
    // {
    //     await RunAsDefaultUserAsync();
    //
    //     var command = new UpdatePositionCommand
    //     {
    //         ClassId = Guid.NewGuid(),
    //         UserId = Guid.NewGuid(),
    //         Position = ClassShareMode.Teacher.ToString()
    //     };
    //
    //     var ex = await FluentActions.Invoking((() => SendAsync(command)))
    //         .Should().ThrowAsync<ErrorCodeException>();
    //
    //     ex.Which.Errors.Should().ContainKey(ErrorCodes.CLASS_NOTFOUND);
    // }

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

        var command = new UpdatePositionCommand
        {
            ClassId = classEntity.Id,
            UserId = studentId,
            Position = ClassShareMode.Teacher.ToString()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.ONLY_OWNERS_CAN_UPDATE);
    }

    [Test]
    public async Task ShouldThrowErrorWhenStudentNotFoundInClass()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var command = new UpdatePositionCommand
        {
            ClassId = classEntity.Id,
            UserId = Guid.NewGuid(), // Non-existent student in class
            Position = ClassShareMode.Teacher.ToString()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.NOT_FOUND_STUDENT_IN_CLASS);
    }

    [Test]
    public async Task ShouldUpdateStudentPositionSuccessfully()
    {
        var ownerId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = ownerId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = ownerId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var studentId = await RunAsUserAsync("student@local", "Student1234!", []);
        await AddAsync(new ClassUser { UserId = studentId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Student });
        await RunAsDefaultUserAsync();
        var command = new UpdatePositionCommand
        {
            ClassId = classEntity.Id,
            UserId = studentId,
            Position = ClassShareMode.Teacher.ToString()
        };

        var result = await SendAsync(command);

        result.Should().NotBeNull();
        result.UserId.Should().Be(studentId);
        result.Position.Should().Be(ClassShareMode.Teacher.ToString());

        var updatedClassUser = (await QueryListAsync<ClassUser>(x => x.Where(cu => cu.ClassId == classEntity.Id && cu.UserId == studentId))).FirstOrDefault();
        updatedClassUser.Should().NotBeNull();
        updatedClassUser!.ShareMode.Should().Be(ClassShareMode.Teacher);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new UpdatePositionCommand
        {
            ClassId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Position = ClassShareMode.Teacher.ToString()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
