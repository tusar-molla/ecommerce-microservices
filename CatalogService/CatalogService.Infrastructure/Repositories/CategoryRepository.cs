using CatalogService.Application.Interfaces;
using CatalogService.Application.Models;
using CatalogService.Infrastructure.Persistence;
using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CategoryRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(Category category)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO Categories (Id, Name, Description, IsActive, CreatedAt)
            VALUES (@Id, @Name, @Description, @IsActive, @CreatedAt)";

            await connection.ExecuteAsync(sql, category);
            return category.Id;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, Name, Description, IsActive, CreatedAt
            FROM Categories
            WHERE Id = @Id AND IsActive = 1";

            return await connection.QuerySingleOrDefaultAsync<Category>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, Name, Description, IsActive, CreatedAt
            FROM Categories
            WHERE IsActive = 1
            ORDER BY Name";

            return await connection.QueryAsync<Category>(sql);
        }
    }
}
