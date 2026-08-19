using CatalogService.Application.Interfaces;
using CatalogService.Application.Models;
using CatalogService.Infrastructure.Persistence;
using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProductRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<Guid> CreateAsync(Product product)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO Products (Id, Name, Description, Price, Sku, CategoryId, ImageUrl, IsActive, CreatedAt)
            VALUES (@Id, @Name, @Description, @Price, @Sku, @CategoryId, @ImageUrl, @IsActive, @CreatedAt)";

            await connection.ExecuteAsync(sql, product);
            return product.Id;
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, Name, Description, Price, Sku, CategoryId, ImageUrl, IsActive, CreatedAt, UpdatedAt
            FROM Products
            WHERE Id = @Id AND IsActive = 1";

            return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var offset = (pageNumber - 1) * pageSize;

            const string countSql = @"
        SELECT COUNT(1) FROM Products
        WHERE IsActive = 1
          AND (@CategoryId IS NULL OR CategoryId = @CategoryId)";

            const string itemsSql = @"
        SELECT Id, Name, Description, Price, Sku, CategoryId, ImageUrl, IsActive, CreatedAt, UpdatedAt
        FROM Products
        WHERE IsActive = 1
          AND (@CategoryId IS NULL OR CategoryId = @CategoryId)
        ORDER BY CreatedAt DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { CategoryId = categoryId });

            var items = await connection.QueryAsync<Product>(itemsSql, new
            {
                CategoryId = categoryId,
                Offset = offset,
                PageSize = pageSize
            });

            return (items, totalCount);
        }

        public async Task<bool> SkuExistsAsync(string sku)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "SELECT COUNT(1) FROM Products WHERE Sku = @Sku";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Sku = sku });
            return count > 0;
        }

        public async Task UpdateAsync(Product product)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
        UPDATE Products
        SET Name = @Name,
            Description = @Description,
            Price = @Price,
            CategoryId = @CategoryId,
            ImageUrl = @ImageUrl,
            UpdatedAt = @UpdatedAt
        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, product);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
        UPDATE Products
        SET IsActive = 0, UpdatedAt = @UpdatedAt
        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        }
    }
}
