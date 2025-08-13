using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Commands;

using static Testing;

public class DeleteClassCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidClassId()
    {
        await RunAsDefaultUserAsync();

        var command = new DeleteClassCommand
        {
            ClassId = null
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorWhenClassNotFound()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new DeleteClassCommand
        {
            ClassId = Guid.NewGuid()
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

        var command = new DeleteClassCommand
        {
            ClassId = classEntity.Id
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.ONLY_OWNERS_CAN_UPDATE);
    }

    [Test]
    public async Task ShouldSoftDeleteClassSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();
        var classEntity = new Class { Name = "Test Class", CreatedBy = userId, IsDeleted = false };
        await AddAsync(classEntity);
        await AddAsync(new ClassUser { UserId = userId, ClassId = classEntity.Id, ShareMode = ClassShareMode.Owner });

        var command = new DeleteClassCommand
        {
            ClassId = classEntity.Id
        };

        var result = await SendAsync(command);

        result.Should().Be(classEntity.Id);

        var deletedClass = (await QueryListAsync<Class>(x => x.Where(c => c.Id == classEntity.Id))).FirstOrDefault();
        deletedClass.Should().BeNull();
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new DeleteClassCommand
        {
            ClassId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
