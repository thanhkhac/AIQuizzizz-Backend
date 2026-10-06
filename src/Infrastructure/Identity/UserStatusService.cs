using CleanArchitectureBase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitectureBase.Infrastructure.Identity;

public record UserAccessStatus(bool Exists, bool IsDeleted, bool IsBanned, string? BanReason)
{
    public bool IsBlocked => !Exists || IsDeleted || IsBanned;
}

public interface IUserStatusService
{
    Task<UserAccessStatus> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Xoá cache trạng thái của user (gọi khi ban/unban/đăng nhập).</summary>
    void Invalidate(Guid userId);
}

/// <summary>
/// Kiểm tra trạng thái khoá/xoá của user cho mỗi request có token, cache ngắn hạn trong bộ nhớ để không truy vấn DB mỗi request.
/// </summary>
public class UserStatusService : IUserStatusService
{
    public static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly IMemoryCache _cache;
    private readonly ApplicationDbContext _dbContext;

    public UserStatusService(IMemoryCache cache, ApplicationDbContext dbContext)
    {
        _cache = cache;
        _dbContext = dbContext;
    }

    public static string CacheKey(Guid userId) => $"user-status:{userId}";

    public void Invalidate(Guid userId) => _cache.Remove(CacheKey(userId));

    public async Task<UserAccessStatus> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = CacheKey(userId);
        if (_cache.TryGetValue(key, out UserAccessStatus? cached) && cached != null)
            return cached;

        var row = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.IsDeleted, u.IsBanned, u.BanReason })
            .FirstOrDefaultAsync(cancellationToken);

        var status = row == null
            ? new UserAccessStatus(false, false, false, null)
            : new UserAccessStatus(true, row.IsDeleted, row.IsBanned, row.BanReason);

        _cache.Set(key, status, CacheDuration);
        return status;
    }
}
