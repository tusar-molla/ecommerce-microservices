using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Infrastructure.BackgroundJobs
{
    public class CartCleanupJob
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartCleanupJob> _logger;
        private const int AbandonedCartDaysThreshold = 30;

        public CartCleanupJob(ICartRepository cartRepository, ILogger<CartCleanupJob> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            var deletedCount = await _cartRepository.DeleteAbandonedCartsAsync(AbandonedCartDaysThreshold);

            _logger.LogInformation("Cart cleanup job ran: removed {Count} abandoned carts older than {Days} days.",deletedCount, AbandonedCartDaysThreshold);
        }
    }
}
