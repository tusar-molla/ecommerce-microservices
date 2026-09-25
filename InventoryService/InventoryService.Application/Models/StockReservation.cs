using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Models
{
    public class StockReservation
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Reserved;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public enum ReservationStatus
    {
        Reserved,
        Confirmed,
        Released
    }
}
