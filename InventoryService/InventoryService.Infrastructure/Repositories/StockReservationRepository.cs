using Dapper;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using InventoryService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Infrastructure.Repositories
{
    public class StockReservationRepository : IStockReservationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public StockReservationRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(StockReservation reservation)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO StockReservations (Id, OrderId, ProductId, Quantity, Status, CreatedAt)
            VALUES (@Id, @OrderId, @ProductId, @Quantity, @Status, @CreatedAt)";

            await connection.ExecuteAsync(sql, new
            {
                reservation.Id,
                reservation.OrderId,
                reservation.ProductId,
                reservation.Quantity,
                Status = reservation.Status.ToString(),
                reservation.CreatedAt
            });

            return reservation.Id;
        }

        public async Task<IEnumerable<StockReservation>> GetByOrderIdAsync(Guid orderId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, OrderId, ProductId, Quantity, Status, CreatedAt
            FROM StockReservations
            WHERE OrderId = @OrderId";

            return await connection.QueryAsync<StockReservation>(sql, new { OrderId = orderId });
        }
    }
}
