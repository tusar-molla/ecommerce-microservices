using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Application.Interfaces
{
    public interface IOrderServiceClient
    {
        Task<OrderInfo?> GetOrderAsync(Guid orderId);
    }

    public class OrderInfo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
