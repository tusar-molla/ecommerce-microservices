using Dapper;
using IdentityService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Persistence
{
    public class SqlTokenBlocklistService : ITokenBlocklistService
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SqlTokenBlocklistService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task RevokeAsync(string jti, DateTime tokenExpiresAt)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO RevokedAccessTokens (Jti, ExpiresAt)
            VALUES (@Jti, @ExpiresAt)";

            await connection.ExecuteAsync(sql, new { Jti = jti, ExpiresAt = tokenExpiresAt });
        }

        public async Task<bool> IsRevokedAsync(string jti)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT COUNT(1) FROM RevokedAccessTokens WHERE Jti = @Jti";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { Jti = jti });
            return count > 0;
        }
    }
}
