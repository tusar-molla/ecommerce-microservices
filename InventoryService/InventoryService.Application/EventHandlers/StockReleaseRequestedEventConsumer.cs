using ECommerce.Contracts.Events;
using InventoryService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.EventHandlers
{
    public class StockReleaseRequestedEventConsumer : IConsumer<StockReleaseRequestedEvent>
    {
        private readonly IStockReservationRepository _reservationRepository;
        private readonly ILogger<StockReleaseRequestedEventConsumer> _logger;

        public StockReleaseRequestedEventConsumer(
            IStockReservationRepository reservationRepository,
            ILogger<StockReleaseRequestedEventConsumer> logger)
        {
            _reservationRepository = reservationRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockReleaseRequestedEvent> context)
        {
            var released = await _reservationRepository.ReleaseForOrderAsync(context.Message.OrderId);

            _logger.LogInformation(
                "Released {Count} reservation(s) for order {OrderId}", released, context.Message.OrderId);
        }
    }
}
