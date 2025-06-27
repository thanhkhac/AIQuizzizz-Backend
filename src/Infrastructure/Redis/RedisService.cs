using CleanArchitectureBase.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace CleanArchitectureBase.Infrastructure.Redis;

public class RedisService : IRedisService
{
    
    private readonly IDatabase _db;
    private readonly double _expiryInMinutes;
    
    public RedisService(IConfiguration configuration)
    {
        var redis = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]!);
        _db = redis.GetDatabase();
        _expiryInMinutes = double.Parse(configuration["Redis:DefaultExpiryMinutes"]!);
    }
    
    public Task SetStringAsync(string key, string value, TimeSpan? expiry = null)
    => _db.StringSetAsync(key, value, expiry ?? TimeSpan.FromMinutes(_expiryInMinutes));

    public async Task<string?> GetStringAsync(string key)
    => (await _db.StringGetAsync(key)).ToString(); 

    public Task DeleteKeyAsync(string key)
    => _db.KeyDeleteAsync(key);

    public async Task ListLeftPush(string key, string value)
    {
        await _db.ListLeftPushAsync(key, value);
        await _db.KeyExpireAsync(key, TimeSpan.FromMinutes(_expiryInMinutes));  // reset TTL mỗi lần push
    }

    public async  Task<List<string>> ListRangeAsync(string key)
    => (await _db.ListRangeAsync(key)).Select(v => v.ToString()).ToList();
}


