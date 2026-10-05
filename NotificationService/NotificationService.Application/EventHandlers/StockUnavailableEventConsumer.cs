using ECommerce.Contracts.Events;
using MassTransit;
using NotificationService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Application.EventHandlers
{

    public class StockUnavailableEventConsumer : IConsumer<StockUnavailableEvent>
    {
        private readonly INotificationDispatcher _dispatcher;

        public StockUnavailableEventConsumer(INotificationDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task Consume(ConsumeContext<StockUnavailableEvent> context)
        {
            var message = context.Message;

            await _dispatcher.DispatchAsync(
                message.OrderId,
                "OrderCancelledStockUnavailable",
                "Your order was cancelled — item unavailable",
                (user, order) => $@"
                <h2>Hi {user.FullName},</h2>
                <p>Sorry, one or more items in order <strong>{order.Id}</strong> became unavailable before we could reserve them, so the order has been cancelled.</p>
                <p>No payment was taken. Please check the product page for restock updates.</p>");
        }
    }
}
