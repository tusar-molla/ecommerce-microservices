using ECommerce.Contracts.Events;
using InventoryService.Application.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.EventHandlers
{
    public class StockReleaseRequestedEventConsumer : IConsumer<StockReleaseRequestedEvent>
    {
        private readonly IStockRepository _stockRepository;

        public StockReleaseRequestedEventConsumer(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task Consume(ConsumeContext<StockReleaseRequestedEvent> context)
        {
            foreach (var item in context.Message.Items)
            {
                await _stockRepository.ReleaseStockAsync(item.ProductId, item.Quantity);
            }
        }
    }
}
