using OrderService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Guid> CreateAsync(Order order, List<OrderItem> items, OutboxMessage outboxMessage);
        Task<Order?> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderItem>> GetItemsAsync(Guid orderId);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task UpdateStatusAsync(Guid orderId, OrderStatus status);
    }
}
