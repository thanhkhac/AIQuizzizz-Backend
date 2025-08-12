using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Commands;

using static Testing;

public class UpdateClassCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateClassCommand
        {
            ClassId = null,
            Name = "Updated Class Name",
            Topic = "Updated Topic"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldRequireName()
    {
        var userId = await RunAsDefaultUserAsync();

        var classEntity = new Class { Name = "Test Class" };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var command = new UpdateClassCommand
        {
            ClassId = classEntity.Id,
            Name = "",
            Topic = "Updated Topic"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenClassNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateClassCommand
        {
            ClassId = Guid.NewGuid(),
            Name = "Updated Class Name",
            Topic = "Updated Topic"
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

        await RunAsDefaultUserAsync(); // Run as a non-owner user

        var command = new UpdateClassCommand
        {
            ClassId = classEntity.Id,
            Name = "Updated Class Name",
            Topic = "Updated Topic"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.ONLY_OWNERS_CAN_UPDATE);
    }

    [Test]
    public async Task ShouldThrowErrorWhenClassAlreadyExistsWithSameNameForOwner()
    {
        var userId = await RunAsDefaultUserAsync();
        var class1 = new Class { Name = "Class One", CreatedBy = userId };
        var class2 = new Class { Name = "Class Two", CreatedBy = userId };
        await AddAsync(class1);
        await AddAsync(class2);
        await AddAsync(new ClassUser { UserId = userId, ClassId = class1.Id, ShareMode = ClassShareMode.Owner });
        await AddAsync(new ClassUser { UserId = userId, ClassId = class2.Id, ShareMode = ClassShareMode.Owner });

        var command = new UpdateClassCommand
        {
            ClassId = class1.Id,
            Name = "Class Two", // Try to rename Class One to Class Two
            Topic = "Updated Topic"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.CLASS_ALREADY_EXISTS);
    }

    [Test]
    public async Task ShouldUpdateClassSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Original Name", Topic = "Original Topic", CreatedBy = userId };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var command = new UpdateClassCommand
        {
            ClassId = classEntity.Id,
            Name = "Updated Name",
            Topic = "Updated Topic"
        };

        var result = await SendAsync(command);

        result.Should().Be(classEntity.Id);

        var updatedClass = (await QueryListAsync<Class>(x => x.Where(c => c.Id == classEntity.Id))).FirstOrDefault();
        updatedClass.Should().NotBeNull();
        updatedClass!.Name.Should().Be("Updated Name");
        updatedClass.Topic.Should().Be("Updated Topic");
    }


    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new UpdateClassCommand
        {
            ClassId = Guid.NewGuid(),
            Name = "Test",
            Topic = "Test"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
