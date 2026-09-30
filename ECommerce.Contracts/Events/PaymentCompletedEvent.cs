namespace ECommerce.Contracts.Events;

public class PaymentCompletedEvent
{
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal AmountCharged { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}