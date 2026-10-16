using CatalogService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CatalogService.Infrastructure.Caching
{
    public class RedisCacheService : ICacheService
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        private static string VersionKey(string area) => $"{area}:version";

        public async Task<T?> GetOrCreateAsync<T>(string area, string key, Func<Task<T?>> factory, TimeSpan ttl) where T : class
        {
            // If Redis is unavailable, serve from the database instead of failing
            if (!_redis.IsConnected) return await factory();

            string? fullKey = null;
            try
            {
                var db = _redis.GetDatabase();
                var raw = await db.StringGetAsync(VersionKey(area));
                long version = raw.HasValue ? (long)raw : 0;

                // The version is part of the key, so bumping it makes every old entry unreachable at once
                fullKey = $"{area}:v{version}:{key}";

                var cached = await db.StringGetAsync(fullKey);
                if (cached.HasValue)
                {
                    var value = JsonSerializer.Deserialize<T>(cached.ToString(), Json);
                    if (value is not null) return value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for {Area}/{Key}, using the database", area, key);
                fullKey = null;
            }

            var result = await factory();

            // Misses (null) are not cached
            if (result is not null && fullKey is not null)
            {
                try
                {
                    await _redis.GetDatabase().StringSetAsync(fullKey, JsonSerializer.Serialize(result, Json), ttl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Redis write failed for {Area}/{Key}", area, key);
                }
            }

            return result;
        }

        public async Task InvalidateAsync(string area)
        {
            if (!_redis.IsConnected) return;

            try
            {
                await _redis.GetDatabase().StringIncrementAsync(VersionKey(area));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis invalidation failed for {Area}", area);
            }
        }
    }
}
