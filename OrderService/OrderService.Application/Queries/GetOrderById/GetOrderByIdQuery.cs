using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Queries.GetOrderById
{
    public class GetOrderByIdQuery : IRequest<OrderDto?>
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
    }
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
