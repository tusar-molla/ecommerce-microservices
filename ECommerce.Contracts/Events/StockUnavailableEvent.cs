namespace ECommerce.Contracts.Events;

public class StockUnavailableEvent
{
    public Guid OrderId { get; set; }
    public List<Guid> UnavailableProductIds { get; set; } = new();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}