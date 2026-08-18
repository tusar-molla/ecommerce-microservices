using Dapper;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Models;
using IdentityService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, Email, PasswordHash, FullName, Role, CreatedAt
            FROM Users
            WHERE Email = @Email";

            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, Email, PasswordHash, FullName, Role, CreatedAt
            FROM Users
            WHERE Id = @Id";

            return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<Guid> CreateAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO Users (Id, Email, PasswordHash, FullName, Role, CreatedAt)
            VALUES (@Id, @Email, @PasswordHash, @FullName, @Role, @CreatedAt)";

            await connection.ExecuteAsync(sql, new
            {
                user.Id,
                user.Email,
                user.PasswordHash,
                user.FullName,
                Role = user.Role.ToString(),
                user.CreatedAt
            });

            return user.Id;
        }
    }
}

