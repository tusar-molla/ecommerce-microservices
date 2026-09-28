using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Interfaces
{
    public interface IInventoryClient
    {
        Task<IEnumerable<StockLevelInfo>> GetStockLevelsAsync(IEnumerable<Guid> productIds);
    }

    public class StockLevelInfo
    {
        public Guid ProductId { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
