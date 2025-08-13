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
