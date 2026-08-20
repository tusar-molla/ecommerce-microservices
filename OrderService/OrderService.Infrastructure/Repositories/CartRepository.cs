using Dapper;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;
using OrderService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CartRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Cart?> GetByUserIdAsync(Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, UserId, CreatedAt, UpdatedAt
            FROM Carts
            WHERE UserId = @UserId";

            return await connection.QuerySingleOrDefaultAsync<Cart>(sql, new { UserId = userId });
        }

        public async Task<Guid> CreateCartAsync(Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var cartId = Guid.NewGuid();

            const string sql = @"
            INSERT INTO Carts (Id, UserId, CreatedAt)
            VALUES (@Id, @UserId, @CreatedAt)";

            await connection.ExecuteAsync(sql, new { Id = cartId, UserId = userId, CreatedAt = DateTime.UtcNow });

            return cartId;
        }

        public async Task<IEnumerable<CartItem>> GetItemsAsync(Guid cartId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, CartId, ProductId, Quantity, CreatedAt
            FROM CartItems
            WHERE CartId = @CartId";

            return await connection.QueryAsync<CartItem>(sql, new { CartId = cartId });
        }

        public async Task<CartItem?> GetItemAsync(Guid cartId, Guid productId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, CartId, ProductId, Quantity, CreatedAt
            FROM CartItems
            WHERE CartId = @CartId AND ProductId = @ProductId";

            return await connection.QuerySingleOrDefaultAsync<CartItem>(sql, new { CartId = cartId, ProductId = productId });
        }

        public async Task AddOrUpdateItemAsync(CartItem item)
        {
            using var connection = _connectionFactory.CreateConnection();

            var existingItem = await GetItemAsync(item.CartId, item.ProductId);

            if (existingItem is not null)
            {
                const string updateSql = @"
                UPDATE CartItems
                SET Quantity = Quantity + @Quantity
                WHERE CartId = @CartId AND ProductId = @ProductId";

                await connection.ExecuteAsync(updateSql, new
                {
                    CartId = item.CartId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }
            else
            {
                const string insertSql = @"
                INSERT INTO CartItems (Id, CartId, ProductId, Quantity, CreatedAt)
                VALUES (@Id, @CartId, @ProductId, @Quantity, @CreatedAt)";

                await connection.ExecuteAsync(insertSql, item);
            }

            const string touchCartSql = "UPDATE Carts SET UpdatedAt = @UpdatedAt WHERE Id = @CartId";
            await connection.ExecuteAsync(touchCartSql, new { CartId = item.CartId, UpdatedAt = DateTime.UtcNow });
        }

        public async Task RemoveItemAsync(Guid cartId, Guid productId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "DELETE FROM CartItems WHERE CartId = @CartId AND ProductId = @ProductId";
            await connection.ExecuteAsync(sql, new { CartId = cartId, ProductId = productId });
        }

        public async Task ClearCartAsync(Guid cartId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "DELETE FROM CartItems WHERE CartId = @CartId";
            await connection.ExecuteAsync(sql, new { CartId = cartId });
        }
    }
}
