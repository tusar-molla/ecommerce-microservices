using InventoryService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Interfaces
{
    public interface IStockReservationRepository
    {
        Task<Guid> CreateAsync(StockReservation reservation);
        Task<IEnumerable<StockReservation>> GetByOrderIdAsync(Guid orderId);
        Task<ReservationResult> ReserveForOrderAsync(Guid messageId, string consumer, Guid orderId, IReadOnlyList<ReservationItem> items);
    }
}
