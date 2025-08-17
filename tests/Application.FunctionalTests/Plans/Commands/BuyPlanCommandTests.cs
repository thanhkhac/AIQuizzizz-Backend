using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Commands;
using static Testing;


public class BuyPlanCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequirePlanId()
    {
        await RunAsDefaultUserAsync();

        var command = new BuyPlanCommand
        {
            PlanId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new BuyPlanCommand
        {
            PlanId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenPlanNotFound()
    {
        await RunAsDefaultUserAsync();

        var command = new BuyPlanCommand
        {
            PlanId = Guid.NewGuid() // Non-existent PlanId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }

    [Test]
    [TestCaseSource(nameof(InsufficientBalanceTestCases))]
    public async Task ShouldThrowErrorCodeExceptionWhenInsufficientBalance(int userBalance, int planPrice)
    {
        var userId = await RunAsDefaultUserAsync();
        var user = await FindAsync<User>(userId);
        user!.Balance = userBalance;
        await UpdateAsync(user);

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Test Plan",
            Price = planPrice,
            Duration = 1,
            Unit = "Month",
            IsActive = true
        };
        await AddAsync(plan);

        var command = new BuyPlanCommand
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.INSUFFICIENT_BALANCE);
    }

    public static IEnumerable<TestCaseData> InsufficientBalanceTestCases()
    {
        yield return new TestCaseData(0, 100).SetName("User balance 0, Plan price 100");
        yield return new TestCaseData(50, 100).SetName("User balance 50, Plan price 100");
        yield return new TestCaseData(99, 100).SetName("User balance 99, Plan price 100");
    }

    [Test]
    [TestCaseSource(nameof(SuccessfulBuyPlanTestCases))]
    public async Task ShouldBuyPlanSuccessfully(string unit, int duration)
    {
        var userId = await RunAsDefaultUserAsync();
        var user = await FindAsync<User>(userId);
        user!.Balance = 1000; // Ensure sufficient balance
        await UpdateAsync(user);

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = $"Test Plan {unit} {duration}",
            Price = 100,
            Duration = duration,
            Unit = unit,
            IsActive = true
        };
        await AddAsync(plan);

        var command = new BuyPlanCommand
        {
            PlanId = plan.Id
        };

        var subscriptionId = await SendAsync(command);

        var userSubscription = await FindAsync<UserSubscription>(subscriptionId);
        userSubscription.Should().NotBeNull();
        userSubscription!.UserId.Should().Be(userId);
        userSubscription.PlanId.Should().Be(plan.Id);
        userSubscription.Price.Should().Be(plan.Price);
        userSubscription.Duration.Should().Be(plan.Duration);
        userSubscription.Unit.Should().Be(plan.Unit);
        userSubscription.IsActive.Should().BeTrue();

        var updatedUser = await FindAsync<User>(userId);
        updatedUser!.Balance.Should().Be(900); // 1000 - 100

        // Verify DateFinish calculation
        var expectedDateFinish = DateTimeOffset.UtcNow;
        if (unit.ToLower() == "day")
        {
            expectedDateFinish = expectedDateFinish.AddDays(duration);
        }
        else if (unit.ToLower() == "month")
        {
            expectedDateFinish = expectedDateFinish.AddMonths(duration);
        }
        else if (unit.ToLower() == "year")
        {
            expectedDateFinish = expectedDateFinish.AddYears(duration);
        }
        // Allow for a small time difference due to test execution time
        userSubscription.DateFinish.Should().BeCloseTo(expectedDateFinish, TimeSpan.FromSeconds(5));
    }

    public static IEnumerable<TestCaseData> SuccessfulBuyPlanTestCases()
    {
        yield return new TestCaseData("Day", 7).SetName("Buy 7 Days Plan");
        yield return new TestCaseData("Month", 1).SetName("Buy 1 Month Plan");
        yield return new TestCaseData("Year", 1).SetName("Buy 1 Year Plan");
        yield return new TestCaseData("Month", 6).SetName("Buy 6 Months Plan");
    }
}
