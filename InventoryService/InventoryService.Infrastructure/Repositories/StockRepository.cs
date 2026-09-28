using Dapper;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using InventoryService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Infrastructure.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public StockRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Stock?> GetByProductIdAsync(Guid productId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, ProductId, QuantityAvailable, QuantityReserved, UpdatedAt
            FROM Stock
            WHERE ProductId = @ProductId";

            return await connection.QuerySingleOrDefaultAsync<Stock>(sql, new { ProductId = productId });
        }

        public async Task<Guid> CreateAsync(Stock stock)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO Stock (Id, ProductId, QuantityAvailable, QuantityReserved, UpdatedAt)
            VALUES (@Id, @ProductId, @QuantityAvailable, @QuantityReserved, @UpdatedAt)";

            await connection.ExecuteAsync(sql, stock);
            return stock.Id;
        }

        public async Task UpdateAsync(Stock stock)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            UPDATE Stock
            SET QuantityAvailable = @QuantityAvailable,
                QuantityReserved = @QuantityReserved,
                UpdatedAt = @UpdatedAt
            WHERE ProductId = @ProductId";

            await connection.ExecuteAsync(sql, stock);
        }

        public async Task<IEnumerable<Guid>> GetExistingProductIdsAsync(IEnumerable<Guid> productIds)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "SELECT ProductId FROM Stock WHERE ProductId IN @ProductIds";
            return await connection.QueryAsync<Guid>(sql, new { ProductIds = productIds });
        }
        public async Task<bool> TryReserveStockAsync(Guid productId, int quantity)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
        UPDATE Stock
        SET QuantityAvailable = QuantityAvailable - @Quantity,
            QuantityReserved = QuantityReserved + @Quantity,
            UpdatedAt = @UpdatedAt
        WHERE ProductId = @ProductId
          AND QuantityAvailable >= @Quantity";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                ProductId = productId,
                Quantity = quantity,
                UpdatedAt = DateTime.UtcNow
            });

            return rowsAffected > 0;
        }

        public async Task ReleaseStockAsync(Guid productId, int quantity)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
        UPDATE Stock
        SET QuantityAvailable = QuantityAvailable + @Quantity,
            QuantityReserved = QuantityReserved - @Quantity,
            UpdatedAt = @UpdatedAt
        WHERE ProductId = @ProductId";

            await connection.ExecuteAsync(sql, new
            {
                ProductId = productId,
                Quantity = quantity,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<Stock>> GetByProductIdsAsync(IEnumerable<Guid> productIds)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
        SELECT Id, ProductId, QuantityAvailable, QuantityReserved, UpdatedAt
        FROM Stock
        WHERE ProductId IN @ProductIds";

            return await connection.QueryAsync<Stock>(sql, new { ProductIds = productIds });
        }
    }
}
