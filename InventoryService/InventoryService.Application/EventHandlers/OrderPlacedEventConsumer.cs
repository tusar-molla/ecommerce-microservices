using ECommerce.Contracts.Events;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.EventHandlers;

public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IStockReservationRepository _reservationRepository;
    private readonly ILogger<OrderPlacedEventConsumer> _logger;

    public OrderPlacedEventConsumer(IStockReservationRepository reservationRepository,ILogger<OrderPlacedEventConsumer> logger)
    {
        _reservationRepository = reservationRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var order = context.Message;

        var messageId = context.MessageId
            ?? throw new InvalidOperationException("OrderPlaced arrived without a MessageId, so it cannot be de-duplicated.");

        var items = order.Items
            .Select(i => new ReservationItem(i.ProductId, i.Quantity))
            .ToList();

        var result = await _reservationRepository.ReserveForOrderAsync(
            messageId, nameof(OrderPlacedEventConsumer), order.OrderId, items);

        if (result.IsDuplicate)
        {
            _logger.LogInformation(
                "Duplicate OrderPlaced {MessageId} for order {OrderId}: no stock changed, repeating the recorded result",
                messageId, order.OrderId);
        }

        // On a duplicate the recorded result is sent again. That covers a crash after the commit but before the
        // publish, which would otherwise leave the order waiting forever. Order and Payment must tolerate a repeat.
        if (result.Outcome == ReservationOutcome.Reserved)
        {
            await context.Publish(new StockReservedEvent { OrderId = order.OrderId });
        }
        else
        {
            await context.Publish(new StockUnavailableEvent
            {
                OrderId = order.OrderId,
                UnavailableProductIds = result.UnavailableProductIds.ToList()
            });
        }
    }
}