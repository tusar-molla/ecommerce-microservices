using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Models
{
    public record ReservationItem(Guid ProductId, int Quantity);

    public enum ReservationOutcome
    {
        Reserved,
        Unavailable
    }

    public class ReservationResult
    {
        public bool IsDuplicate { get; init; }
        public ReservationOutcome Outcome { get; init; }
        public IReadOnlyList<Guid> UnavailableProductIds { get; init; } = Array.Empty<Guid>();
    }
}
