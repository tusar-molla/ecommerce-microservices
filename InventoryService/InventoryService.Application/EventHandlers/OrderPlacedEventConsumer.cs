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
            var successfullyReservedItems = new List<OrderPlacedItem>();

            foreach (var item in orderEvent.Items)
            {
                var reserved = await _stockRepository.TryReserveStockAsync(item.ProductId, item.Quantity);

                if (reserved)
                {
                    successfullyReservedItems.Add(item);
                }
                else
                {
                    unavailableProductIds.Add(item.ProductId);
                }
            }

            if (unavailableProductIds.Count > 0)
            {
                foreach (var item in successfullyReservedItems)
                {
                    await _stockRepository.ReleaseStockAsync(item.ProductId, item.Quantity);
                }

                await context.Publish(new StockUnavailableEvent
                {
                    OrderId = orderEvent.OrderId,
                    UnavailableProductIds = unavailableProductIds
                });
                return;
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

            await context.Publish(new StockReservedEvent { OrderId = orderEvent.OrderId });
        }
    }
}
