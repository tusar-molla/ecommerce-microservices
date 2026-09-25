using ECommerce.Contracts.Events;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.EventHandlers
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IStockRepository _stockRepository;
        private readonly IStockReservationRepository _stockReservationRepository;

        public OrderPlacedEventConsumer(
            IStockRepository stockRepository,
            IStockReservationRepository stockReservationRepository)
        {
            _stockRepository = stockRepository;
            _stockReservationRepository = stockReservationRepository;
        }
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var orderEvent = context.Message;

            var unavailableProductIds = new List<Guid>();
            var stockUpdates = new List<Stock>();

            foreach (var item in orderEvent.Items)
            {
                var stock = await _stockRepository.GetByProductIdAsync(item.ProductId);

                if (stock is null || stock.QuantityAvailable < item.Quantity)
                {
                    unavailableProductIds.Add(item.ProductId);
                    continue;
                }

                stock.QuantityAvailable -= item.Quantity;
                stock.QuantityReserved += item.Quantity;
                stock.UpdatedAt = DateTime.UtcNow;
                stockUpdates.Add(stock);
            }

            if (unavailableProductIds.Count > 0)
            {
                await context.Publish(new StockUnavailableEvent
                {
                    OrderId = orderEvent.OrderId,
                    UnavailableProductIds = unavailableProductIds
                });
                return;
            }

            foreach (var stock in stockUpdates)
            {
                await _stockRepository.UpdateAsync(stock);
            }

            foreach (var item in orderEvent.Items)
            {
                await _stockReservationRepository.CreateAsync(new StockReservation
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderEvent.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            await context.Publish(new StockReservedEvent
            {
                OrderId = orderEvent.OrderId
            });
        }
    }
}
