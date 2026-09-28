using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Queries.GetStockLevels
{
    public class GetStockLevelsQuery : IRequest<IEnumerable<StockLevelDto>>
    {
        public List<Guid> ProductIds { get; set; } = new();
    }

    public class StockLevelDto
    {
        public Guid ProductId { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
