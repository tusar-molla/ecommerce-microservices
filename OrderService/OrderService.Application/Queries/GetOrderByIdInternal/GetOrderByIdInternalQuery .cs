using MediatR;
using OrderService.Application.Queries.GetOrderById;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Queries.GetOrderByIdInternal
{
    public class GetOrderByIdInternalQuery : IRequest<OrderDto?>
    {
        public Guid OrderId { get; set; }
    }
}
