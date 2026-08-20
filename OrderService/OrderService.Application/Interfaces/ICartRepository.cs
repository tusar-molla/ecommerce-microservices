using OrderService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task<Guid> CreateCartAsync(Guid userId);
        Task<IEnumerable<CartItem>> GetItemsAsync(Guid cartId);
        Task<CartItem?> GetItemAsync(Guid cartId, Guid productId);
        Task AddOrUpdateItemAsync(CartItem item);
        Task RemoveItemAsync(Guid cartId, Guid productId);
        Task ClearCartAsync(Guid cartId);
    }
}
