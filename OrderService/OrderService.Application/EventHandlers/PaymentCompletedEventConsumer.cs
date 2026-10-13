using ECommerce.Contracts.Events;
using MassTransit;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.EventHandlers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public PaymentCompletedEventConsumer(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            await _orderRepository.TryAdvanceStatusAsync(
                context.Message.OrderId, OrderStatus.Confirmed, OrderStatus.Pending, OrderStatus.StockReserved);
        }
    }
}
