namespace ECommerce.Contracts.Events;

public class StockReservedEvent
{
    public Guid OrderId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}