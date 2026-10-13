using ECommerce.Contracts.Events;
using MassTransit;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;

namespace OrderService.Application.EventHandlers;

public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IOrderRepository _orderRepository;

    public PaymentFailedEventConsumer(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var orderId = context.Message.OrderId;

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
        {
            return;
        }

        var items = await _orderRepository.GetItemsAsync(orderId);

        var release = new StockReleaseRequestedEvent
        {
            OrderId = orderId,
            Items = items.Select(i => new StockReleaseItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        await _orderRepository.TryCancelAsync(orderId, OutboxMessage.From(release));
    }
}