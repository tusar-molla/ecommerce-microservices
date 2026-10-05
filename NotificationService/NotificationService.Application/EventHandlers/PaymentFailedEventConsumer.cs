using ECommerce.Contracts.Events;
using MassTransit;
using NotificationService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Application.EventHandlers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly INotificationDispatcher _dispatcher;

        public PaymentFailedEventConsumer(INotificationDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var message = context.Message;

            await _dispatcher.DispatchAsync(
                message.OrderId,
                "OrderCancelledPaymentFailed",
                "Your order was cancelled — payment failed",
                (user, order) => $@"
                <h2>Hi {user.FullName},</h2>
                <p>Unfortunately we could not process the payment for order <strong>{order.Id}</strong>, so the order has been cancelled.</p>
                <p>The items have been released and you have not been charged. You are welcome to place the order again.</p>");
        }
    }
}
