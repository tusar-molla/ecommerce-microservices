using InventoryService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Interfaces
{
    public interface IStockRepository
    {
        Task<Stock?> GetByProductIdAsync(Guid productId);
        Task<Guid> CreateAsync(Stock stock);
        Task UpdateAsync(Stock stock);
    }
}
