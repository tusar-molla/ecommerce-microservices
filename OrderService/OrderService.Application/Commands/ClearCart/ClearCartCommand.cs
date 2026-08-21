using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.ClearCart
{
    public class ClearCartCommand : IRequest<Unit>
    {
        public Guid UserId { get; set; }
    }
}
