using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using static CleanArchitectureBase.Application.Command.UnitTests.Testing;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Queries;

public class GetPlatformOverviewQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireAdministratorOrModeratorRole()
    {
        await RunAsDefaultUserAsync(); // Not Administrator or Moderator

        var query = new GetPlatformOverviewQuery();

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION);
    }

    // [Test]
    // public async Task ShouldReturnPlatformOverviewData()
    // {
    //     await RunAsAdministratorAsync();
    //
    //     // Arrange: Add some test data
    //     var user1 = new User { Id = Guid.NewGuid(), Email = "Hello",FullName = "User 1", Balance = 100 };
    //     var user2 = new User { Id = Guid.NewGuid(), FullName = "User 2", Balance = 200 };
    //     var user3 = new User { Id = Guid.NewGuid(), FullName = "User 3", Balance = 300 };
    //     await AddAsync(user1);
    //     await AddAsync(user2);
    //     await AddAsync(user3);
    //
    //     var class1 = new Class { Id = Guid.NewGuid(), Name = "Class A" };
    //     var class2 = new Class { Id = Guid.NewGuid(), Name = "Class B" };
    //     await AddAsync(class1);
    //     await AddAsync(class2);
    //
    //     var plan1 = new Plan { Id = Guid.NewGuid(), Name = "Plan X", Price = 50, Duration = 1, Unit = "Month" };
    //     var plan2 = new Plan { Id = Guid.NewGuid(), Name = "Plan Y", Price = 75, Duration = 1, Unit = "Month" };
    //     await AddAsync(plan1);
    //     await AddAsync(plan2);
    //
    //     await AddAsync(new UserSubscription { Id = Guid.NewGuid(), UserId = user1.Id, PlanId = plan1.Id, Price = plan1.Price, DateStart = DateTimeOffset.UtcNow, DateFinish = DateTimeOffset.UtcNow.AddMonths(1) });
    //     await AddAsync(new UserSubscription { Id = Guid.NewGuid(), UserId = user2.Id, PlanId = plan2.Id, Price = plan2.Price, DateStart = DateTimeOffset.UtcNow, DateFinish = DateTimeOffset.UtcNow.AddMonths(1) });
    //     await AddAsync(new UserSubscription { Id = Guid.NewGuid(), UserId = user1.Id, PlanId = plan2.Id, Price = plan2.Price, DateStart = DateTimeOffset.UtcNow, DateFinish = DateTimeOffset.UtcNow.AddMonths(1) }); // User 1 buys another plan
    //
    //     var query = new GetPlatformOverviewQuery();
    //
    //     var result = await SendAsync(query);
    //
    //     result.Should().NotBeNull();
    //     result.Users.Should().Be(3); // user1, user2, user3
    //     result.Classes.Should().Be(2); // class1, class2
    //     result.Revenue.Should().Be(Convert.ToInt32(plan1.Price + plan2.Price + plan2.Price)); // 50 + 75 + 75 = 200
    //     result.UsersHavePlan.Should().Be(2); // user1, user2
    // }
}
