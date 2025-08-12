using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Commands;

using static Testing;

public class DeletePlanCommandTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldSoftDeletePlanSuccessfully_WhenUserIsAdmin()
    {
        await RunAsAdministratorAsync();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan to Delete",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false,
            CanLearn = false,
            CanOpenTest = false,
            CanCopyOrImportQuestionSet = false
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

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenPlanIdIsEmpty()
    {
        await RunAsAdministratorAsync();
        var command = new DeletePlanCommand
        {
            PlanId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenPlanNotFound()
    {
        await RunAsAdministratorAsync();
        var command = new DeletePlanCommand
        {
            PlanId = Guid.NewGuid()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUserIsNotAdmin()
    {
        await RunAsDefaultUserAsync();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan to Delete",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        await AddAsync(plan);

        var command = new DeletePlanCommand
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan to Delete",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        await AddAsync(plan);


        var command = new DeletePlanCommand
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
