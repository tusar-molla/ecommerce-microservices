using ECommerce.Contracts.Events;
using MassTransit;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.EventHandlers
{
    public class StockUnavailableEventConsumer : IConsumer<StockUnavailableEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public StockUnavailableEventConsumer(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }


        public async Task Consume(ConsumeContext<StockUnavailableEvent> context)
        {
            await _orderRepository.TryAdvanceStatusAsync(
                context.Message.OrderId, OrderStatus.Cancelled, OrderStatus.Pending);
        }
    }
}
