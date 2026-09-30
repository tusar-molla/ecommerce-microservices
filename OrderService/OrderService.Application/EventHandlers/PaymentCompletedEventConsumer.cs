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
            var order = await _orderRepository.GetByIdAsync(context.Message.OrderId);
            if (order is null)
            {
                return;
            }

            await _orderRepository.UpdateStatusAsync(order.Id, OrderStatus.Confirmed);
        }
    }
}
