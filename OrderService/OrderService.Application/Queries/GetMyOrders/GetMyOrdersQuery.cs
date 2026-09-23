using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Queries.GetMyOrders
{
    public class GetMyOrdersQuery : IRequest<IEnumerable<OrderSummaryDto>>
    {
        public Guid UserId { get; set; }
    }
    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
