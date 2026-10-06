using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Tests;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.UnitTests.Tests;

[TestFixture]
public class GetTestScheduleQueryTests
{
    private ApplicationDbContext _db = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private GetTestScheduleQueryHandler CreateHandler(Guid? userId = null)
    {
        var user = new Mock<IUser>();
        user.SetupGet(x => x.UserId).Returns(userId ?? _userId);
        return new GetTestScheduleQueryHandler(_db, user.Object);
    }

    private async Task<Test> AddTestAsync(ClassShareMode? mode, DateTimeOffset start, bool classDeleted = false, bool testDeleted = false)
    {
        var cls = new Class { Name = "Class", IsDeleted = classDeleted };
        if (mode.HasValue)
            cls.ClassUsers.Add(new ClassUser { ClassId = cls.Id, UserId = _userId, ShareMode = mode.Value });
        _db.Classes.Add(cls);

        var test = new Test
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            ClassId = cls.Id,
            TimeStart = start,
            TimeFinish = start.AddHours(1),
            TimeLimit = 30,
            QuestionCount = 1,
            GradeAttemptMethod = GradeAttemptMethod.HighestScore,
            GradeQuestionMethod = GradeQuestionMethod.Partial,
            IsDeleted = testDeleted
        };
        _db.Tests.Add(test);
        await _db.SaveChangesAsync();
        return test;
    }

    private static List<Application.Tests.Dto.TestScheduleDto> Flatten(List<Application.Tests.Dto.TestScheduleResponse> r)
        => r.SelectMany(x => x.TestSchedules).ToList();

    [TestCase(ClassShareMode.Student)]
    [TestCase(ClassShareMode.Teacher)]
    [TestCase(ClassShareMode.Owner)]
    public async Task Member_OfAnyRole_SeesTestsOfTheirClass(ClassShareMode mode)
    {
        var test = await AddTestAsync(mode, new DateTimeOffset(2026, 10, 15, 3, 0, 0, TimeSpan.Zero));

        var result = await CreateHandler().Handle(new GetTestScheduleQuery { Month = 10, Year = 2026 }, default);

        Flatten(result).Select(x => x.TestId).Should().ContainSingle().Which.Should().Be(test.Id);
    }

    [Test]
    public async Task NonMember_SeesNothing()
    {
        await AddTestAsync(null, new DateTimeOffset(2026, 10, 15, 3, 0, 0, TimeSpan.Zero));

        var result = await CreateHandler().Handle(new GetTestScheduleQuery { Month = 10, Year = 2026 }, default);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task DeletedClassOrTest_IsExcluded()
    {
        await AddTestAsync(ClassShareMode.Owner, new DateTimeOffset(2026, 10, 15, 3, 0, 0, TimeSpan.Zero), classDeleted: true);
        await AddTestAsync(ClassShareMode.Owner, new DateTimeOffset(2026, 10, 15, 3, 0, 0, TimeSpan.Zero), testDeleted: true);

        var result = await CreateHandler().Handle(new GetTestScheduleQuery { Month = 10, Year = 2026 }, default);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task ReturnsRealTimeStartAndFinish_AsDateTimeOffset()
    {
        var start = new DateTimeOffset(2026, 10, 1, 20, 38, 0, TimeSpan.Zero); // 02/10 03:38 ở UTC+7
        await AddTestAsync(ClassShareMode.Student, start);

        var dto = Flatten(await CreateHandler().Handle(new GetTestScheduleQuery { Month = 10, Year = 2026 }, default)).Single();

        dto.TimeStart.Should().Be(start);
        dto.TimeFinish.Should().Be(start.AddHours(1));
    }

    [Test]
    public async Task TestsNearMonthBorder_AreIncludedForOtherTimeZones()
    {
        // 30/09 20:00Z = 01/10 03:00 (UTC+7) -> thuộc tháng 10 với người dùng UTC+7
        var borderStart = new DateTimeOffset(2026, 9, 30, 20, 0, 0, TimeSpan.Zero);
        // 31/10 20:00Z = 01/11 03:00 (UTC+7) -> thuộc tháng 11, vẫn được trả về (client tự lọc)
        var borderEnd = new DateTimeOffset(2026, 10, 31, 20, 0, 0, TimeSpan.Zero);
        var far = new DateTimeOffset(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);
        var t1 = await AddTestAsync(ClassShareMode.Student, borderStart);
        var t2 = await AddTestAsync(ClassShareMode.Student, borderEnd);
        await AddTestAsync(ClassShareMode.Student, far);

        var result = await CreateHandler().Handle(new GetTestScheduleQuery { Month = 10, Year = 2026 }, default);

        Flatten(result).Select(x => x.TestId).Should().BeEquivalentTo(new[] { t1.Id, t2.Id });
    }

    [Test]
    public async Task December_RangeCrossesYearBoundary()
    {
        var t = await AddTestAsync(ClassShareMode.Teacher, new DateTimeOffset(2026, 12, 31, 20, 0, 0, TimeSpan.Zero));

        var result = await CreateHandler().Handle(new GetTestScheduleQuery { Month = 12, Year = 2026 }, default);

        Flatten(result).Select(x => x.TestId).Should().Contain(t.Id);
    }
}
