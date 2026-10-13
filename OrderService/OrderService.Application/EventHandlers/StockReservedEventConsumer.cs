using ECommerce.Contracts.Events;
using MassTransit;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.EventHandlers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public StockReservedEventConsumer(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            await _orderRepository.TryAdvanceStatusAsync(
                context.Message.OrderId, OrderStatus.StockReserved, OrderStatus.Pending);
        }
    }
}
