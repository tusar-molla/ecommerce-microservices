using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Queries.GetCart
{
    public class GetCartQuery : IRequest<CartDto>
    {
        public Guid UserId { get; set; }
    }

    public class CartDto
    {
        public Guid CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.LineTotal);
    }

    public class CartItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
