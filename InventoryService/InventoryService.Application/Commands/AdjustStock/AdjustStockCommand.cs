using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Commands.AdjustStock
{
    public class AdjustStockCommand : IRequest<Unit>
    {
        public Guid ProductId { get; set; }
        public int QuantityChange { get; set; } // positive to add stock, negative to remove
    }
}
