using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Commands.SetInitialStock
{
    public class SetInitialStockCommand : IRequest<Guid>
    {
        public Guid ProductId { get; set; }
        public int InitialQuantity { get; set; }
    }
}
