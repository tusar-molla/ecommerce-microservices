using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Models
{
    public class Stock
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantityReserved { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
