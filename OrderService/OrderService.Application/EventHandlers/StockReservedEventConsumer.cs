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
            var order = await _orderRepository.GetByIdAsync(context.Message.OrderId);
            if (order is null)
            {
                return;
            }

            order.Status = OrderStatus.StockReserved;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateStatusAsync(order.Id, order.Status);
        }
    }
}
