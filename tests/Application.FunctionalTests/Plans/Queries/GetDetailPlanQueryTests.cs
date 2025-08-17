using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using static CleanArchitectureBase.Application.Command.UnitTests.Testing;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Queries;

public class GetDetailPlanQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequirePlanId()
    {
        await RunAsDefaultUserAsync();

        var query = new GetDetailPlanQuery
        {
            PlanId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var query = new GetDetailPlanQuery
        {
            PlanId = Guid.NewGuid()
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenPlanNotFound()
    {
        await RunAsDefaultUserAsync();

        var query = new GetDetailPlanQuery
        {
            PlanId = Guid.NewGuid() // Non-existent PlanId
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }

    [Test]
    public async Task ShouldReturnPlanDetailDtoForExistingPlan()
    {
        await RunAsDefaultUserAsync();

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Detail Test Plan",
            Price = 500,
            Duration = 6,
            Unit = "Month",
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = false,
            IsActive = true,
            IsDeleted = false
        };
        await AddAsync(plan);

        var query = new GetDetailPlanQuery
        {
            PlanId = plan.Id
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Id.Should().Be(plan.Id);
        result.Name.Should().Be(plan.Name);
        result.Price.Should().Be(plan.Price);
        result.Duration.Should().Be(plan.Duration);
        result.Unit.Should().Be(plan.Unit);
        result.CanLearn.Should().Be(plan.CanLearn);
        result.CanOpenTest.Should().Be(plan.CanOpenTest);
        result.CanCopyOrImportQuestionSet.Should().Be(plan.CanCopyOrImportQuestionSet);
        result.IsActive.Should().Be(plan.IsActive);
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenPlanIsDeleted()
    {
        await RunAsDefaultUserAsync();

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true,
            IsDeleted = true // Deleted plan
        };
        await AddAsync(plan);

        var query = new GetDetailPlanQuery
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }
}
