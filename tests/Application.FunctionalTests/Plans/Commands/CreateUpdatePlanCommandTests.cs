using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Commands;
using static Testing;
public class CreateUpdatePlanCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireAdministratorOrModeratorRole()
    {
        await RunAsDefaultUserAsync(); // Not Administrator or Moderator

        var command = new CreateUpdatePlanCommand
        {
            Name = "Test Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.USER_NOT_HAVE_PERMISSION);
    }

    [Test]
    [TestCaseSource(nameof(InvalidCreateUpdatePlanCommandData))]
    public async Task ShouldRejectInvalidCreateUpdatePlanCommand(CreateUpdatePlanCommand command, string expectedErrorCode)
    {
        await RunAsAdministratorAsync();

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(expectedErrorCode);
    }

    public static IEnumerable<TestCaseData> InvalidCreateUpdatePlanCommandData()
    {
        yield return new TestCaseData(
            new CreateUpdatePlanCommand
            {
                Name = "",
                Price = 100,
                Duration = 1,
                Unit = "Month"
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Name empty");

        yield return new TestCaseData(
            new CreateUpdatePlanCommand
            {
                Name = "Test Plan",
                Price = -10,
                Duration = 1,
                Unit = "Month"
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Price negative");

        yield return new TestCaseData(
            new CreateUpdatePlanCommand
            {
                Name = "Test Plan",
                Price = 100,
                Duration = 0,
                Unit = "Month"
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Duration zero");

        yield return new TestCaseData(
            new CreateUpdatePlanCommand
            {
                Name = "Test Plan",
                Price = 100,
                Duration = 1,
                Unit = "InvalidUnit"
            },
            ErrorCodes.COMMON_INVALID_MODEL
        ).SetName("Unit invalid");
    }

    [Test]
    public async Task ShouldCreateNewPlanSuccessfully()
    {
        await RunAsAdministratorAsync();

        var command = new CreateUpdatePlanCommand
        {
            Name = "New Plan",
            Price = 200,
            Duration = 3,
            Unit = "Month",
            CanLearn = true,
            CanOpenTest = false,
            CanCopyOrImportQuestionSet = true,
            IsActive = true
        };

        var planId = await SendAsync(command);

        var plan = await FindAsync<Plan>(planId);
        plan.Should().NotBeNull();
        plan!.Name.Should().Be(command.Name);
        plan.Price.Should().Be(command.Price);
        plan.Duration.Should().Be(command.Duration);
        plan.Unit.Should().Be(command.Unit);
        plan.CanLearn.Should().Be(command.CanLearn);
        plan.CanOpenTest.Should().Be(command.CanOpenTest);
        plan.CanCopyOrImportQuestionSet.Should().Be(command.CanCopyOrImportQuestionSet);
        plan.IsActive.Should().Be(command.IsActive);

        var priceHistory = await QueryListAsync<PlanPriceHistory>(
            x => x.Where(h => h.PlanId == planId));
        priceHistory.Should().ContainSingle();
        priceHistory.First().Price.Should().Be(command.Price);
        priceHistory.First().DateFinish.Should().BeNull();
    }

    [Test]
    public async Task ShouldUpdateExistingPlanSuccessfully()
    {
        await RunAsAdministratorAsync();

        var existingPlan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Old Plan",
            Price = 100,
            Duration = 1,
            Unit = "Year",
            CanLearn = false,
            CanOpenTest = false,
            CanCopyOrImportQuestionSet = false,
            IsActive = true
        };
        await AddAsync(existingPlan);

        var command = new CreateUpdatePlanCommand
        {
            PlanId = existingPlan.Id,
            Name = "Updated Plan",
            Price = 100, // Price remains the same
            Duration = 6,
            Unit = "Month",
            CanLearn = true,
            CanOpenTest = true,
            CanCopyOrImportQuestionSet = true,
            IsActive = false
        };

        var planId = await SendAsync(command);

        planId.Should().Be(existingPlan.Id);

        var updatedPlan = await FindAsync<Plan>(planId);
        updatedPlan.Should().NotBeNull();
        updatedPlan!.Name.Should().Be(command.Name);
        updatedPlan.Price.Should().Be(command.Price);
        updatedPlan.Duration.Should().Be(command.Duration);
        updatedPlan.Unit.Should().Be(command.Unit);
        updatedPlan.CanLearn.Should().Be(command.CanLearn);
        updatedPlan.CanOpenTest.Should().Be(command.CanOpenTest);
        updatedPlan.CanCopyOrImportQuestionSet.Should().Be(command.CanCopyOrImportQuestionSet);
        updatedPlan.IsActive.Should().Be(command.IsActive);

        var priceHistory = await QueryListAsync<PlanPriceHistory>(
            x => x.Where(h => h.PlanId == planId));
        priceHistory.Should().ContainSingle(); // Price didn't change, so no new history entry
        priceHistory.First().Price.Should().Be(existingPlan.Price);
    }

    [Test]
    public async Task ShouldCreateNewPlanPriceHistoryWhenPriceChanges()
    {
        await RunAsAdministratorAsync();

        var existingPlan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "Old Plan",
            Price = 100,
            Duration = 1,
            Unit = "Year",
            IsActive = true
        };
        await AddAsync(existingPlan);

        var initialPriceHistory = new PlanPriceHistory
        {
            PlanId = existingPlan.Id,
            Price = existingPlan.Price,
            DateStart = DateTimeOffset.UtcNow.AddDays(-10),
            DateFinish = null
        };
        await AddAsync(initialPriceHistory);

        var command = new CreateUpdatePlanCommand
        {
            PlanId = existingPlan.Id,
            Name = "Updated Plan",
            Price = 150, // Price changes
            Duration = 1,
            Unit = "Year",
            IsActive = true
        };

        var planId = await SendAsync(command);

        planId.Should().Be(existingPlan.Id);

        var updatedPlan = await FindAsync<Plan>(planId);
        updatedPlan.Should().NotBeNull();
        updatedPlan!.Price.Should().Be(command.Price);

        var priceHistories = await QueryListAsync<PlanPriceHistory>(
            x => x.Where(h => h.PlanId == planId).OrderBy(h => h.DateStart));
        priceHistories.Should().HaveCount(2);

        priceHistories[0].Price.Should().Be(100);
        priceHistories[0].DateFinish.Should().NotBeNull();
        priceHistories[1].Price.Should().Be(150);
        priceHistories[1].DateFinish.Should().BeNull();
    }

    [Test]
    public async Task ShouldThrowErrorCodeExceptionWhenUpdatingNonExistentPlan()
    {
        await RunAsAdministratorAsync();

        var command = new CreateUpdatePlanCommand
        {
            PlanId = Guid.NewGuid(), // Non-existent PlanId
            Name = "Non Existent Plan",
            Price = 100,
            Duration = 1,
            Unit = "Month"
        };

        var ex = await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ErrorCodeException>();

        ex.Which.Errors.Should().ContainKey(ErrorCodes.PLAN_NOT_FOUND);
    }
}
