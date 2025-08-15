using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using static CleanArchitectureBase.Application.Command.UnitTests.Testing;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Queries;

public class GetRevenueByYearQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireAdministratorOrModeratorRole()
    {
        await RunAsDefaultUserAsync(); // Not Administrator or Moderator

        var query = new GetRevenueByYearQuery
        {
            Year = DateTime.UtcNow.Year
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION);
    }

    [Test]
    [TestCaseSource(nameof(InvalidYearTestCases))]
    public async Task ShouldRequireYearGreaterThanOrEqualTo2000(int year)
    {
        await RunAsAdministratorAsync();

        var query = new GetRevenueByYearQuery
        {
            Year = year
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    public static IEnumerable<TestCaseData> InvalidYearTestCases()
    {
        yield return new TestCaseData(1999).SetName("Year less than 2000");
        yield return new TestCaseData(0).SetName("Year zero");
        yield return new TestCaseData(-1).SetName("Year negative");
    }

    // [Test]
    // public async Task ShouldReturnRevenueByMonthForGivenYear()
    // {
    //     await RunAsAdministratorAsync();
    //
    //     var year = DateTimeOffset.UtcNow.Year;
    //
    //     // Add some user subscriptions for testing
    //     var user1 = new User { Id = Guid.NewGuid(), FullName = "User 1", Balance = 1000 };
    //     var user2 = new User { Id = Guid.NewGuid(), FullName = "User 2", Balance = 1000 };
    //     await AddAsync(user1);
    //     await AddAsync(user2);
    //
    //     var plan1 = new Plan { Id = Guid.NewGuid(), Name = "Plan A", Price = 50, Duration = 1, Unit = "Month" };
    //     var plan2 = new Plan { Id = Guid.NewGuid(), Name = "Plan B", Price = 75, Duration = 1, Unit = "Month" };
    //     await AddAsync(plan1);
    //     await AddAsync(plan2);
    //
    //     await AddAsync(new UserSubscription { Id = Guid.NewGuid(), UserId = user1.Id, PlanId = plan1.Id, Price = plan1.Price, DateStart = new DateTimeOffset(year, 1, 15, 0, 0, 0, TimeSpan.Zero) });
    //     await AddAsync(new UserSubscription { Id = Guid.NewGuid(), UserId = user2.Id, PlanId = plan2.Id, Price = plan2.Price, DateStart = new DateTimeOffset(year, 1, 20, 0, 0, 0, TimeSpan.Zero) });
    //     await AddAsync(new UserSubscription { Id = Guid.NewGuid(), UserId = user1.Id, PlanId = plan1.Id, Price = plan1.Price, DateStart = new DateTimeOffset(year, 3, 10, 0, 0, 0, TimeSpan.Zero) });
    //     await AddAsync(new UserSubscription { Id = Guid.NewGuid(), UserId = user2.Id, PlanId = plan2.Id, Price = plan2.Price, DateStart = new DateTimeOffset(year, 3, 25, 0, 0, 0, TimeSpan.Zero) });
    //     await AddAsync(new UserSubscription { Id = Guid.NewGuid(), UserId = user1.Id, PlanId = plan1.Id, Price = plan1.Price, DateStart = new DateTimeOffset(year - 1, 12, 1, 0, 0, 0, TimeSpan.Zero) }); // Subscription from previous year
    //
    //     var query = new GetRevenueByYearQuery
    //     {
    //         Year = year
    //     };
    //
    //     var result = await SendAsync(query);
    //
    //     result.Should().NotBeNull();
    //     result.Should().HaveCount(12); // All 12 months should be present
    //
    //     result.First(x => x.Month == 1).Revenue.Should().Be(Convert.ToInt32(plan1.Price + plan2.Price)); // 50 + 75 = 125
    //     result.First(x => x.Month == 2).Revenue.Should().Be(0);
    //     result.First(x => x.Month == 3).Revenue.Should().Be(Convert.ToInt32(plan1.Price + plan2.Price)); // 50 + 75 = 125
    //     result.First(x => x.Month == 4).Revenue.Should().Be(0);
    //     // ... and so on for other months
    //     result.First(x => x.Month == 12).Revenue.Should().Be(0);
    // }
}
