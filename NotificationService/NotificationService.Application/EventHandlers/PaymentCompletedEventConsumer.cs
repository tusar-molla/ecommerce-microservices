using ECommerce.Contracts.Events;
using MassTransit;
using NotificationService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Application.EventHandlers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly INotificationDispatcher _dispatcher;

        public PaymentCompletedEventConsumer(INotificationDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var message = context.Message;

            await _dispatcher.DispatchAsync(
                message.OrderId,
                "OrderConfirmed",
                "Your order has been confirmed",
                (user, order) => $@"
                <h2>Thank you, {user.FullName}!</h2>
                <p>We received your payment of <strong>{message.AmountCharged:N2} BDT</strong>.</p>
                <p>Your order <strong>{order.Id}</strong> is confirmed and being prepared.</p>");
        }
    }
}
