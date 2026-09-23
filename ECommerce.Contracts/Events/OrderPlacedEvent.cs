using System;

public class OrderPlacedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public List<OrderPlacedItem> Items { get; set; } = new();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public class OrderPlacedItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}