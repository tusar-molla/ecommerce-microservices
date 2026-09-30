namespace ECommerce.Contracts.Events;

public class StockReleaseRequestedEvent
{
    public Guid OrderId { get; set; }
    public List<StockReleaseItem> Items { get; set; } = new();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public class StockReleaseItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}