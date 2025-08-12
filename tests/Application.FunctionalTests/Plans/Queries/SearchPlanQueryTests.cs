using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Queries;

using static Testing;

public class SearchPlanQueryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReturnAllActiveAndInactivePlans_WhenNoFilter()
    {
        var plan1 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Active Plan 1",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        var plan2 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Plan",
            Price = 200,
            Duration = 1,
            Unit = "year",
            IsActive = false,
            IsDeleted = false
        };
        var plan3 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Plan",
            Price = 50,
            Duration = 1,
            Unit = "day",
            IsActive = true,
            IsDeleted = true
        };
        await AddAsync(plan1);
        await AddAsync(plan2);
        await AddAsync(plan3);

        var query = new SearchPlanQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == plan1.Id);
        result.Should().Contain(p => p.Id == plan2.Id);
        result.Should().NotContain(p => p.Id == plan3.Id);
    }

    //normal
    [Test]
    public async Task ShouldReturnOnlyActivePlans_WhenFilterByIsActiveTrue()
    {
        var plan1 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Active Plan 1",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        var plan2 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Plan",
            Price = 200,
            Duration = 1,
            Unit = "year",
            IsActive = false,
            IsDeleted = false
        };
        await AddAsync(plan1);
        await AddAsync(plan2);

        var query = new SearchPlanQuery
        {
            IsActive = true
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == plan1.Id);
        result.Should().NotContain(p => p.Id == plan2.Id);
    }

    //normal
    [Test]
    public async Task ShouldReturnOnlyInactivePlans_WhenFilterByIsActiveFalse()
    {
        var plan1 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Active Plan 1",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        var plan2 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Plan",
            Price = 200,
            Duration = 1,
            Unit = "year",
            IsActive = false,
            IsDeleted = false
        };
        await AddAsync(plan1);
        await AddAsync(plan2);

        var query = new SearchPlanQuery
        {
            IsActive = false
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.Should().Contain(p => p.Id == plan2.Id);
        result.Should().NotContain(p => p.Id == plan1.Id);
    }

    //normal
    [Test]
    public async Task ShouldReturnPlansSortedByPrice()
    {
        var plan1 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan C",
            Price = 300,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        var plan2 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan A",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        var plan3 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Plan B",
            Price = 200,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        await AddAsync(plan1);
        await AddAsync(plan2);
        await AddAsync(plan3);

        var query = new SearchPlanQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.First().Id.Should().Be(plan2.Id);
        result[1].Id.Should().Be(plan3.Id);
        result.Last().Id.Should().Be(plan1.Id);
    }

    //normal
    [Test]
    public async Task ShouldReturnEmptyList_WhenNoPlansExist()
    {
        var query = new SearchPlanQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    //normal
    [Test]
    public async Task ShouldReturnEmptyList_WhenNoMatchingPlansFound()
    {
        var plan1 = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Active Plan",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = false
        };
        await AddAsync(plan1);

        var query = new SearchPlanQuery
        {
            IsActive = false
        };
        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
