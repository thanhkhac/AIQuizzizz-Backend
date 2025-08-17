using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using static CleanArchitectureBase.Application.Command.UnitTests.Testing;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Commands;

public class DeletePlanCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireAdministratorOrModeratorRole()
    {
        await RunAsDefaultUserAsync(); // Not Administrator or Moderator

        var command = new DeletePlanCommand
        {
            PlanId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION);
    }

    [Test]
    public async Task ShouldRequirePlanId()
    {
        await RunAsAdministratorAsync();

        var command = new DeletePlanCommand
        {
            PlanId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenPlanNotFound()
    {
        await RunAsAdministratorAsync();

        var command = new DeletePlanCommand
        {
            PlanId = Guid.NewGuid() // Non-existent PlanId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }

    [Test]
    public async Task ShouldDeletePlanSuccessfully()
    {
        await RunAsAdministratorAsync();

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan to Delete",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true,
            IsDeleted = false
        };
        await AddAsync(plan);

        var command = new DeletePlanCommand
        {
            PlanId = plan.Id
        };

        var deletedPlanId = await SendAsync(command);

        deletedPlanId.Should().Be(plan.Id);

        var deletedPlan = await FindAsync<Plan>(plan.Id);
        deletedPlan.Should().NotBeNull();
        deletedPlan!.IsDeleted.Should().BeTrue();
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenPlanAlreadyDeleted()
    {
        await RunAsAdministratorAsync();

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Already Deleted Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true,
            IsDeleted = true // Already deleted
        };
        await AddAsync(plan);

        var command = new DeletePlanCommand
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }
}
