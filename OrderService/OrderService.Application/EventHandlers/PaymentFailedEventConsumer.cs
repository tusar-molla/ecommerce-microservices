using ECommerce.Contracts.Events;
using MassTransit;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.EventHandlers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly IOrderRepository _orderRepository;

        public PaymentFailedEventConsumer(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var order = await _orderRepository.GetByIdAsync(context.Message.OrderId);
            if (order is null)
            {
                return;
            }

            await _orderRepository.UpdateStatusAsync(order.Id, OrderStatus.Cancelled);

            var items = await _orderRepository.GetItemsAsync(order.Id);

            await context.Publish(new StockReleaseRequestedEvent
            {
                OrderId = order.Id,
                Items = items.Select(i => new StockReleaseItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            });
        }
    }
}
