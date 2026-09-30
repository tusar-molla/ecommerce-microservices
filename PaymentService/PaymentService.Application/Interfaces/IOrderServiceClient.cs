using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Interfaces
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
        public string Status { get; set; } = string.Empty;
    }
}
