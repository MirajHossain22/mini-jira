using System.Text.Json;
using StackExchange.Redis;

namespace TaskTracker.Api.Services;

public sealed class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : ICacheService
{
    private IDatabase Db => redis.GetDatabase();
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try { var value = await Db.StringGetAsync(key); return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default; }
        catch (RedisException ex) { logger.LogWarning(ex, "Redis read failed for {Key}", key); return default; }
    }
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct = default)
    {
        try { await Db.StringSetAsync(key, JsonSerializer.Serialize(value), expiration); }
        catch (RedisException ex) { logger.LogWarning(ex, "Redis write failed for {Key}", key); }
    }
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try { await Db.KeyDeleteAsync(key); }
        catch (RedisException ex) { logger.LogWarning(ex, "Redis delete failed for {Key}", key); }
    }
    public async Task<long> GetVersionAsync(string scope, CancellationToken ct = default)
    {
        try { var value = await Db.StringGetAsync($"version:{scope}"); return value.TryParse(out long version) ? version : 0; }
        catch (RedisException ex) { logger.LogWarning(ex, "Redis version read failed"); return DateTimeOffset.UtcNow.ToUnixTimeSeconds(); }
    }
    public async Task BumpVersionAsync(string scope, CancellationToken ct = default)
    {
        try { await Db.StringIncrementAsync($"version:{scope}"); }
        catch (RedisException ex) { logger.LogWarning(ex, "Redis version bump failed"); }
    }
}
