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
}
}
