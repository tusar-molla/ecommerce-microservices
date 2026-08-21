using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.RemoveFromCart
{
    public class RemoveFromCartCommand : IRequest<Unit>
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
    }
}
