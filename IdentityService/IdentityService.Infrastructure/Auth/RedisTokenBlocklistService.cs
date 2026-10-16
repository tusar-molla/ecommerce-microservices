using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Auth
{
    public class RedisTokenBlocklistService : ITokenBlocklistService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly SqlTokenBlocklistService _sql;
        private readonly ILogger<RedisTokenBlocklistService> _logger;

        public RedisTokenBlocklistService(
            IConnectionMultiplexer redis,
            SqlTokenBlocklistService sql,
            ILogger<RedisTokenBlocklistService> logger)
        {
            _redis = redis;
            _sql = sql;
            _logger = logger;
        }

        public async Task RevokeAsync(string jti, DateTime tokenExpiresAt)
        {
            // SQL keeps the durable copy
            await _sql.RevokeAsync(jti, tokenExpiresAt);

            try
            {
                // Redis removes the entry itself when the token would have expired anyway
                var ttl = tokenExpiresAt.ToUniversalTime() - DateTime.UtcNow;
                if (ttl > TimeSpan.Zero)
                {
                    await _redis.GetDatabase().StringSetAsync($"revoked-jti:{jti}", "1", ttl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not write the revoked token to Redis; the SQL copy was saved");
            }
        }

        public async Task<bool> IsRevokedAsync(string jti)
        {
            try
            {
                if (_redis.IsConnected)
                {
                    return await _redis.GetDatabase().KeyExistsAsync($"revoked-jti:{jti}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis check failed; falling back to SQL");
            }

            return await _sql.IsRevokedAsync(jti);
        }
    }
}
