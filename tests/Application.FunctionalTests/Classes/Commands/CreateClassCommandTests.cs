using CleanArchitectureBase.Application.Classes;
using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Classes.Commands;

using static Testing;

public class CreateClassCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireName()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateClassCommand
        {
            Name = "",
            Topic = "Test Topic"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldCreateClassSuccessfully()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateClassCommand
        {
            Name = "New Test Class",
            Topic = "Test Topic"
        };

        var classId = await SendAsync(command);

        var newClass = await FindAsync<Class>(classId);
        newClass.Should().NotBeNull();
        newClass!.Name.Should().Be(command.Name);
        newClass.Topic.Should().Be(command.Topic);
        newClass.CreatedBy.Should().Be(userId);

        var classUser = await FindAsync<ClassUser>(classId, userId);
        classUser.Should().NotBeNull();
        classUser!.ShareMode.Should().Be(ClassShareMode.Owner);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new CreateClassCommand
        {
            Name = "New Test Class",
            Topic = "Test Topic"
        };

        var ex = await FluentActions.Invoking((() => SendAsync(command)))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
