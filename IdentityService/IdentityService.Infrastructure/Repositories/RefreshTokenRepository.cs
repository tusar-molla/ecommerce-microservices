using Dapper;
using IdentityService.Application.DTOs;
using IdentityService.Application.Models;
using IdentityService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task CreateAsync(RefreshToken refreshToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO RefreshTokens (Id, UserId, Token, ExpiresAt, CreatedAt, RevokedAt)
            VALUES (@Id, @UserId, @Token, @ExpiresAt, @CreatedAt, @RevokedAt)";

            await connection.ExecuteAsync(sql, refreshToken);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, UserId, Token, ExpiresAt, CreatedAt, RevokedAt
            FROM RefreshTokens
            WHERE Token = @Token";

            return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task RevokeAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            UPDATE RefreshTokens
            SET RevokedAt = @RevokedAt
            WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new { Id = id, RevokedAt = DateTime.UtcNow });
        }
    }
}
