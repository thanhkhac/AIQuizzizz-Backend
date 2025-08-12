using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Commands;

using static Testing;

public class CreateUpdatePlanCommandTests : BaseTestFixture
{
    //normal
    [Test]
    public async Task ShouldCreatePlanSuccessfully()
    {
        await RunAsAdministratorAsync();
        var command = new CreateUpdatePlanCommand
        {
            Name = "New Monthly Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            CanLearn = true,
            CanOpenTest = false,
            CanCopyOrImportQuestionSet = true,
            IsActive = true
        };

        var planId = await SendAsync(command);

        planId.Should().NotBeEmpty();
        var newPlan = await FindAsync<Plan>(planId);
        newPlan.Should().NotBeNull();
        newPlan!.Name.Should().Be(command.Name);
        newPlan.Price.Should().Be(command.Price);
        newPlan.Duration.Should().Be(command.Duration);
        newPlan.Unit.Should().Be(command.Unit);
        newPlan.CanLearn.Should().Be(command.CanLearn);
        newPlan.CanOpenTest.Should().Be(command.CanOpenTest);
        newPlan.CanCopyOrImportQuestionSet.Should().Be(command.CanCopyOrImportQuestionSet);
        newPlan.IsActive.Should().Be(command.IsActive);

        var priceHistory = await QueryListAsync<PlanPriceHistory>(ph => ph.Where(x => x.PlanId == planId));
        priceHistory.Should().HaveCount(1);
        priceHistory.First().Price.Should().Be(command.Price);
        priceHistory.First().DateStart.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        priceHistory.First().DateFinish.Should().BeNull();
    }

    //normal
    [Test]
    public async Task ShouldUpdatePlanSuccessfully_WhenPriceNotChanged()
    {
        await RunAsAdministratorAsync();
        var existingPlan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Existing Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true,
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = true
        };
        await AddAsync(existingPlan);
        await AddAsync(new PlanPriceHistory
        {
            PlanId = existingPlan.Id,
            Price = existingPlan.Price,
            DateStart = DateTimeOffset.UtcNow.AddDays(-10)
        });

        var command = new CreateUpdatePlanCommand
        {
            PlanId = existingPlan.Id,
            Name = "Updated Plan Name",
            Price = 100,
            Duration = 2,
            Unit = "Year",
            CanLearn = false,
            CanOpenTest = false,
            CanCopyOrImportQuestionSet = false,
            IsActive = false
        };

        var planId = await SendAsync(command);

        planId.Should().Be(existingPlan.Id);
        var updatedPlan = await FindAsync<Plan>(planId);
        updatedPlan.Should().NotBeNull();
        updatedPlan!.Name.Should().Be(command.Name);
        updatedPlan.Duration.Should().Be(command.Duration);
        updatedPlan.Unit.Should().Be(command.Unit);
        updatedPlan.CanLearn.Should().Be(command.CanLearn);
        updatedPlan.CanOpenTest.Should().Be(command.CanOpenTest);
        updatedPlan.CanCopyOrImportQuestionSet.Should().Be(command.CanCopyOrImportQuestionSet);
        updatedPlan.IsActive.Should().Be(command.IsActive);

        var priceHistory = await QueryListAsync<PlanPriceHistory>(ph => ph.Where(x => x.PlanId == planId));
        priceHistory.Should().HaveCount(1);
        priceHistory.First().Price.Should().Be(command.Price);
    }

    //normal
    [Test]
    public async Task ShouldUpdatePlanSuccessfully_WhenPriceChanged()
    {
        await RunAsAdministratorAsync();
        var existingPlan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Existing Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true,
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = true
        };
        await AddAsync(existingPlan);
        await AddAsync(new PlanPriceHistory
        {
            PlanId = existingPlan.Id,
            Price = existingPlan.Price,
            DateStart = DateTimeOffset.UtcNow.AddDays(-10)
        });

        var command = new CreateUpdatePlanCommand
        {
            PlanId = existingPlan.Id,
            Name = "Updated Plan Name",
            Price = 150,
            Duration = 1,
            Unit = "Month",
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = true,
            IsActive = true
        };

        var planId = await SendAsync(command);

        planId.Should().Be(existingPlan.Id);
        var updatedPlan = await FindAsync<Plan>(planId);
        updatedPlan.Should().NotBeNull();
        updatedPlan!.Price.Should().Be(command.Price);

        var priceHistory = await QueryListAsync<PlanPriceHistory>(ph => ph.Where(x => x.PlanId == planId).OrderByDescending(x => x.DateStart));
        priceHistory.Should().HaveCount(2);
        priceHistory.First().Price.Should().Be(command.Price);
        priceHistory.First().DateStart.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        priceHistory.First().DateFinish.Should().BeNull();
        priceHistory.Last().Price.Should().Be(existingPlan.Price);
        priceHistory.Last().DateFinish.Should().NotBeNull();
        priceHistory.Last().DateFinish.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenPlanIdNotFoundOnUpdate()
    {
        await RunAsAdministratorAsync();
        var command = new CreateUpdatePlanCommand
        {
            PlanId = Guid.NewGuid(),
            Name = "Non Existent Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenNameIsEmpty()
    {
        await RunAsAdministratorAsync();
        var command = new CreateUpdatePlanCommand
        {
            Name = "",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    [TestCase(-1)]
    public async Task ShouldThrowError_WhenPriceIsNegative(int price)
    {
        await RunAsAdministratorAsync();
        var command = new CreateUpdatePlanCommand
        {
            Name = "Test Plan",
            Price = price,
            Duration = 1,
            Unit = "Month",
            IsActive = true
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public async Task ShouldThrowError_WhenDurationIsZeroOrLess(int duration)
    {
        await RunAsAdministratorAsync();
        var command = new CreateUpdatePlanCommand
        {
            Name = "Test Plan",
            Price = 100,
            Duration = duration,
            Unit = "Month",
            IsActive = true
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowError_WhenUnitIsInvalid()
    {
        await RunAsAdministratorAsync();
        var command = new CreateUpdatePlanCommand
        {
            Name = "Test Plan",
            Price = 100,
            Duration = 1,
            Unit = "InvalidUnit",
            IsActive = true
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_INVALID_MODEL);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotLoggedIn()
    {
        var command = new CreateUpdatePlanCommand
        {
            Name = "Test Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_UNAUTHORIZED);
    }

    //abnormal
    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenNotAdmin()
    {
        await RunAsDefaultUserAsync();
        var command = new CreateUpdatePlanCommand
        {
            Name = "Test Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month",
            IsActive = true
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ErrorCodeException>();
        ex.Which.Errors.Should().ContainKey(ErrorCodes.COMMON_FORBIDDEN);
    }
}
