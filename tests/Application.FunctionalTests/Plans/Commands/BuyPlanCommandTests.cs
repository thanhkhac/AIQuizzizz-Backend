using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Commands;

using static Testing;

public class BuyPlanCommandTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldBuyPlanSuccessfully_WhenUserHasSufficientBalance()
    {
        var userId = await RunAsDefaultUserAsync();
        var user = await FindAsync<User>(userId);
        user!.Balance = 1000;
        await UpdateAsync(user);
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Monthly Plan",
            Price = 500,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = true
        };
        await AddAsync(plan);

        var command = new BuyPlanCommand
        {
            PlanId = plan.Id
        };

        var subscriptionId = await SendAsync(command);

        subscriptionId.Should().NotBeEmpty();
        var newSubscription = await FindAsync<UserSubscription>(subscriptionId);
        newSubscription.Should().NotBeNull();
        newSubscription!.UserId.Should().Be(userId);
        newSubscription.PlanId.Should().Be(plan.Id);
        newSubscription.Price.Should().Be(plan.Price);
        newSubscription.Duration.Should().Be(plan.Duration);
        newSubscription.Unit.Should().Be(plan.Unit);
        newSubscription.IsActive.Should().BeTrue();
        newSubscription.DateStart.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        newSubscription.DateFinish.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMonths(1), TimeSpan.FromSeconds(5));

        var updatedUser = await FindAsync<User>(userId);
        updatedUser!.Balance.Should().Be(500);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenPlanIdIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var command = new BuyPlanCommand
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
        var userId = await RunAsDefaultUserAsync();
        var user = await FindAsync<User>(userId);
        user!.Balance = 1000;
        await UpdateAsync(user);

        var command = new BuyPlanCommand
        {
            PlanId = Guid.NewGuid()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenInsufficientBalance()
    {
        var userId = await RunAsDefaultUserAsync();
        var user = await FindAsync<User>(userId);
        user!.Balance = 100;
        await UpdateAsync(user);

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Monthly Plan",
            Price = 500,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = true
        };
        await AddAsync(plan);

        var command = new BuyPlanCommand
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.INSUFFICIENT_BALANCE);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Monthly Plan",
            Price = 500,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = true
        };
        await AddAsync(plan);


        var command = new BuyPlanCommand
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
