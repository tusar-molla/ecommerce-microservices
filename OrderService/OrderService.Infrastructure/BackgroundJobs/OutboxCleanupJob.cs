using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Infrastructure.BackgroundJobs
{
    public class OutboxCleanupJob
    {
        private readonly IOutboxRepository _outboxRepository;
        private readonly ILogger<OutboxCleanupJob> _logger;
        private readonly int _retentionDays;

        public OutboxCleanupJob(
            IOutboxRepository outboxRepository,
            IConfiguration configuration,
            ILogger<OutboxCleanupJob> logger)
        {
            _outboxRepository = outboxRepository;
            _logger = logger;
            _retentionDays = int.TryParse(configuration["Outbox:RetentionDays"], out var days) && days > 0 ? days : 7;
        }

        public async Task RunAsync()
        {
            var deleted = await _outboxRepository.DeleteProcessedAsync(_retentionDays);

            _logger.LogInformation(
                "Outbox cleanup removed {Count} processed messages older than {Days} days.",
                deleted, _retentionDays);
        }
    }
}
