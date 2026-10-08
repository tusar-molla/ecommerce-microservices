using ECommerce.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace OrderService.Infrastructure.Messaging
{
    public class OutboxPublisherService : BackgroundService
    {
        private const int BatchSize = 20;
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan PublishTimeout = TimeSpan.FromSeconds(10);

        // Every event class lives in ECommerce.Contracts, so stored type names are resolved from there
        private static readonly Assembly ContractsAssembly = typeof(OrderPlacedEvent).Assembly;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherService> _logger;

        public OutboxPublisherService(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox publisher started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Outbox cycle failed");
                }

                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ProcessBatchAsync(CancellationToken stoppingToken)
        {
            // The worker is a singleton, but the repository and publish endpoint are scoped, so each pass gets its own scope
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var messages = await repository.GetUnprocessedAsync(BatchSize);

            foreach (var message in messages)
            {
                var eventType = ContractsAssembly.GetType(message.EventType, throwOnError: false);
                if (eventType is null)
                {
                    await repository.MarkDeadAsync(message.Id, $"Unknown event type '{message.EventType}'");
                    continue;
                }

                object? payload;
                try
                {
                    payload = JsonSerializer.Deserialize(message.Payload, eventType);
                }
                catch (JsonException ex)
                {
                    await repository.MarkDeadAsync(message.Id, $"Invalid payload: {ex.Message}");
                    continue;
                }

                if (payload is null)
                {
                    await repository.MarkDeadAsync(message.Id, "Payload deserialized to null");
                    continue;
                }

                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    timeout.CancelAfter(PublishTimeout);

                    // The outbox row id becomes the message id, so consumers can recognise a repeat delivery
                    await publisher.Publish(
                        payload,
                        eventType,
                        Pipe.Execute<PublishContext>(ctx => ctx.MessageId = message.Id),
                        timeout.Token);

                    await repository.MarkProcessedAsync(message.Id);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    await repository.RecordFailureAsync(message.Id, ex.Message);
                    _logger.LogWarning(ex, "Could not publish outbox message {MessageId}, will retry", message.Id);
                    break;
                }
            }
        }
    }
}
