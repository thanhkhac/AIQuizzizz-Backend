using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Entities;
using static CleanArchitectureBase.Application.Command.UnitTests.Testing;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Queries;

public class SearchPlanQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnAllActivePlansWhenIsActiveIsTrue()
    {
        await RunAsDefaultUserAsync();

        // Arrange
        var activePlan1 = new Plan { Id = Guid.NewGuid(), Name = "Active Plan 1", Price = 100, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = false };
        var activePlan2 = new Plan { Id = Guid.NewGuid(), Name = "Active Plan 2", Price = 200, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = false };
        var inactivePlan = new Plan { Id = Guid.NewGuid(), Name = "Inactive Plan", Price = 150, Duration = 1, Unit = "Month", IsActive = false, IsDeleted = false };
        var deletedPlan = new Plan { Id = Guid.NewGuid(), Name = "Deleted Plan", Price = 50, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = true };

        await AddAsync(activePlan1);
        await AddAsync(activePlan2);
        await AddAsync(inactivePlan);
        await AddAsync(deletedPlan);

        var query = new SearchPlanQuery { IsActive = true };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == activePlan1.Id);
        result.Should().Contain(p => p.Id == activePlan2.Id);
        result.Should().NotContain(p => p.Id == inactivePlan.Id);
        result.Should().NotContain(p => p.Id == deletedPlan.Id);
        result.Should().BeInAscendingOrder(p => p.Price);
    }

    [Test]
    public async Task ShouldReturnAllInactivePlansWhenIsActiveIsFalse()
    {
        await RunAsDefaultUserAsync();

        // Arrange
        var activePlan = new Plan { Id = Guid.NewGuid(), Name = "Active Plan", Price = 100, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = false };
        var inactivePlan1 = new Plan { Id = Guid.NewGuid(), Name = "Inactive Plan 1", Price = 150, Duration = 1, Unit = "Month", IsActive = false, IsDeleted = false };
        var inactivePlan2 = new Plan { Id = Guid.NewGuid(), Name = "Inactive Plan 2", Price = 250, Duration = 1, Unit = "Month", IsActive = false, IsDeleted = false };
        var deletedPlan = new Plan { Id = Guid.NewGuid(), Name = "Deleted Plan", Price = 50, Duration = 1, Unit = "Month", IsActive = false, IsDeleted = true };

        await AddAsync(activePlan);
        await AddAsync(inactivePlan1);
        await AddAsync(inactivePlan2);
        await AddAsync(deletedPlan);

        var query = new SearchPlanQuery { IsActive = false };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == inactivePlan1.Id);
        result.Should().Contain(p => p.Id == inactivePlan2.Id);
        result.Should().NotContain(p => p.Id == activePlan.Id);
        result.Should().NotContain(p => p.Id == deletedPlan.Id);
        result.Should().BeInAscendingOrder(p => p.Price);
    }

    [Test]
    public async Task ShouldReturnAllPlansWhenIsActiveIsNull()
    {
        await RunAsDefaultUserAsync();

        // Arrange
        var activePlan = new Plan { Id = Guid.NewGuid(), Name = "Active Plan", Price = 100, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = false };
        var inactivePlan = new Plan { Id = Guid.NewGuid(), Name = "Inactive Plan", Price = 150, Duration = 1, Unit = "Month", IsActive = false, IsDeleted = false };
        var deletedPlan = new Plan { Id = Guid.NewGuid(), Name = "Deleted Plan", Price = 50, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = true };

        await AddAsync(activePlan);
        await AddAsync(inactivePlan);
        await AddAsync(deletedPlan);

        var query = new SearchPlanQuery { IsActive = null };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Should not include deleted plan
        result.Should().Contain(p => p.Id == activePlan.Id);
        result.Should().Contain(p => p.Id == inactivePlan.Id);
        result.Should().NotContain(p => p.Id == deletedPlan.Id);
        result.Should().BeInAscendingOrder(p => p.Price);
    }

    [Test]
    public async Task ShouldNotReturnDeletedPlans()
    {
        await RunAsDefaultUserAsync();

        // Arrange
        var activePlan = new Plan { Id = Guid.NewGuid(), Name = "Active Plan", Price = 100, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = false };
        var deletedPlan = new Plan { Id = Guid.NewGuid(), Name = "Deleted Plan", Price = 50, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = true };

        await AddAsync(activePlan);
        await AddAsync(deletedPlan);

        var query = new SearchPlanQuery { IsActive = null }; // Search all

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == activePlan.Id);
        result.Should().NotContain(p => p.Id == deletedPlan.Id);
    }

    [Test]
    public async Task ShouldOrderPlansByPrice()
    {
        await RunAsDefaultUserAsync();

        // Arrange
        var plan1 = new Plan { Id = Guid.NewGuid(), Name = "Plan C", Price = 300, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = false };
        var plan2 = new Plan { Id = Guid.NewGuid(), Name = "Plan A", Price = 100, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = false };
        var plan3 = new Plan { Id = Guid.NewGuid(), Name = "Plan B", Price = 200, Duration = 1, Unit = "Month", IsActive = true, IsDeleted = false };

        await AddAsync(plan1);
        await AddAsync(plan2);
        await AddAsync(plan3);

        var query = new SearchPlanQuery { IsActive = null };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(p => p.Price).Should().ContainInOrder(100, 200, 300);
    }
}
