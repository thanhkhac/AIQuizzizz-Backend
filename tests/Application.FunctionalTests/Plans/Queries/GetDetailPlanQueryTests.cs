using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Queries;

using static Testing;

public class GetDetailPlanQueryTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldReturnPlanDetail_WhenPlanExistsAndIsNotDeleted()
    {
        await RunAsDefaultUserAsync();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Test Plan",
            Price = 100,
            Duration = 1,
            Unit = "month",
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = true,
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

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenPlanIdIsEmpty()
    {
        await RunAsDefaultUserAsync();
        var query = new GetDetailPlanQuery
        {
            PlanId = Guid.Empty
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenPlanNotFound()
    {
        await RunAsDefaultUserAsync();
        var query = new GetDetailPlanQuery
        {
            PlanId = Guid.NewGuid()
        };
        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenPlanIsDeleted()
    {
        await RunAsDefaultUserAsync();
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Plan",
            Price = 100,
            Duration = 1,
            Unit = "month",
            IsActive = true,
            IsDeleted = true,
            CanLearn = false,
            CanOpenTest = false,
            CanCopyOrImportQuestionSet = false
        };
        await AddAsync(plan);

        var query = new GetDetailPlanQuery
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Test Plan",
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


        var query = new GetDetailPlanQuery
        {
            PlanId = plan.Id
        };

        var ex = await FluentActions.Invoking(() => SendAsync(query)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }
}
