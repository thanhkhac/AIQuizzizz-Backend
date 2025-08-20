using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Plans;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;
using static CleanArchitectureBase.Application.Command.UnitTests.Testing;

namespace CleanArchitectureBase.Application.Command.UnitTests.Plans.Queries;

public class GetNumberOfNewClassByYearQueryTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireAdministratorOrModeratorRole()
    {
        await RunAsDefaultUserAsync(); // Not Administrator or Moderator

        var query = new GetNumberOfNewClassByYearQuery
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

        var query = new GetNumberOfNewClassByYearQuery
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

    [Test]
    public async Task ShouldReturnNumberOfNewClassesByMonthForGivenYear()
    {
        await RunAsAdministratorAsync();

        var year = DateTime.UtcNow.Year;

        // Add some classes for testing
        await AddAsync(new Class { Id = Guid.NewGuid(), Name = "Class 1", Created = new DateTimeOffset(year, 1, 15, 0, 0, 0, TimeSpan.Zero) });
        await AddAsync(new Class { Id = Guid.NewGuid(), Name = "Class 2", Created = new DateTimeOffset(year, 1, 20, 0, 0, 0, TimeSpan.Zero) });
        await AddAsync(new Class { Id = Guid.NewGuid(), Name = "Class 3", Created = new DateTimeOffset(year, 3, 10, 0, 0, 0, TimeSpan.Zero) });
        await AddAsync(new Class { Id = Guid.NewGuid(), Name = "Class 4", Created = new DateTimeOffset(year, 3, 25, 0, 0, 0, TimeSpan.Zero) });
        await AddAsync(new Class { Id = Guid.NewGuid(), Name = "Class 5", Created = new DateTimeOffset(year, 3, 30, 0, 0, 0, TimeSpan.Zero) });
        await AddAsync(new Class { Id = Guid.NewGuid(), Name = "Class 6", Created = new DateTimeOffset(year - 1, 1, 1, 0, 0, 0, TimeSpan.Zero) }); // Class from previous year

        var query = new GetNumberOfNewClassByYearQuery
        {
            Year = year
        };

        var result = await SendAsync(query);

        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThan(0);
    }
}
