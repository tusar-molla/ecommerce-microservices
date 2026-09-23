using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.PlaceOrder
{
    public class PlaceOrderCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
    }
}
