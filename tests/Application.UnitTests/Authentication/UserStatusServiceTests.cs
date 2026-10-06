using CleanArchitectureBase.Infrastructure.Data;
using CleanArchitectureBase.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace CleanArchitectureBase.Application.UnitTests.Authentication;

[TestFixture]
public class UserStatusServiceTests
{
    private ApplicationDbContext _db = null!;
    private MemoryCache _cache = null!;
    private UserStatusService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new UserStatusService(_cache, _db);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _cache.Dispose();
    }

    [Test]
    public async Task BannedUser_IsBlocked_WithReason()
    {
        var id = Guid.NewGuid();
        _db.Users.Add(new UserAccount { Id = id, UserName = "a", IsBanned = true, BanReason = "spam" });
        await _db.SaveChangesAsync();

        var status = await _service.GetStatusAsync(id);

        status.IsBlocked.Should().BeTrue();
        status.IsBanned.Should().BeTrue();
        status.BanReason.Should().Be("spam");
    }

    [Test]
    public async Task ActiveUser_IsNotBlocked()
    {
        var id = Guid.NewGuid();
        _db.Users.Add(new UserAccount { Id = id, UserName = "b" });
        await _db.SaveChangesAsync();

        (await _service.GetStatusAsync(id)).IsBlocked.Should().BeFalse();
    }

    [Test]
    public async Task DeletedOrMissingUser_IsBlocked()
    {
        var id = Guid.NewGuid();
        _db.Users.Add(new UserAccount { Id = id, UserName = "c", IsDeleted = true });
        await _db.SaveChangesAsync();

        (await _service.GetStatusAsync(id)).IsBlocked.Should().BeTrue();
        (await _service.GetStatusAsync(Guid.NewGuid())).IsBlocked.Should().BeTrue();
    }

    [Test]
    public async Task Status_IsCachedBetweenCalls()
    {
        var id = Guid.NewGuid();
        var user = new UserAccount { Id = id, UserName = "d" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        (await _service.GetStatusAsync(id)).IsBlocked.Should().BeFalse();

        user.IsBanned = true;
        await _db.SaveChangesAsync();

        (await _service.GetStatusAsync(id)).IsBlocked.Should().BeFalse("ket qua duoc cache 30s");
    }

    [Test]
    public async Task Invalidate_ReloadsStatus_AfterBanAndUnban()
    {
        var id = Guid.NewGuid();
        var user = new UserAccount { Id = id, UserName = "e" };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        (await _service.GetStatusAsync(id)).IsBlocked.Should().BeFalse();

        user.IsBanned = true;
        await _db.SaveChangesAsync();
        _service.Invalidate(id);
        (await _service.GetStatusAsync(id)).IsBanned.Should().BeTrue();

        user.IsBanned = false;
        await _db.SaveChangesAsync();
        (await _service.GetStatusAsync(id)).IsBanned.Should().BeTrue("van la ket qua cache cho den khi invalidate");
        _service.Invalidate(id);
        (await _service.GetStatusAsync(id)).IsBlocked.Should().BeFalse();
    }
}
