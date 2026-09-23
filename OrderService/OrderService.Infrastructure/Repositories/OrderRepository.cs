using Dapper;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;
using OrderService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OrderRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(Order order, List<OrderItem> items)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string orderSql = @"
                INSERT INTO Orders (Id, UserId, Status, TotalAmount, ShippingAddress, CreatedAt)
                VALUES (@Id, @UserId, @Status, @TotalAmount, @ShippingAddress, @CreatedAt)";

                await connection.ExecuteAsync(orderSql, new
                {
                    order.Id,
                    order.UserId,
                    Status = order.Status.ToString(),
                    order.TotalAmount,
                    order.ShippingAddress,
                    order.CreatedAt
                }, transaction);

                const string itemSql = @"
                INSERT INTO OrderItems (Id, OrderId, ProductId, ProductName, UnitPrice, Quantity)
                VALUES (@Id, @OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity)";

                foreach (var item in items)
                {
                    await connection.ExecuteAsync(itemSql, new
                    {
                        item.Id,
                        item.OrderId,
                        item.ProductId,
                        item.ProductName,
                        item.UnitPrice,
                        item.Quantity
                    }, transaction);
                }

                transaction.Commit();
                return order.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, UserId, Status, TotalAmount, ShippingAddress, CreatedAt, UpdatedAt
            FROM Orders
            WHERE Id = @Id";

            return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });
        }

        public async Task<IEnumerable<OrderItem>> GetItemsAsync(Guid orderId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, OrderId, ProductId, ProductName, UnitPrice, Quantity
            FROM OrderItems
            WHERE OrderId = @OrderId";

            return await connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId });
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, UserId, Status, TotalAmount, ShippingAddress, CreatedAt, UpdatedAt
            FROM Orders
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC";

            return await connection.QueryAsync<Order>(sql, new { UserId = userId });
        }
    }
}

