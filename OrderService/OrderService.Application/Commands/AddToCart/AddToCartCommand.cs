using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.AddToCart
{
    public class AddToCartCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
