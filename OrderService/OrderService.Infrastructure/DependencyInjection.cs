using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.EventHandlers;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.BackgroundJobs;
using OrderService.Infrastructure.ExternalServices;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is missing.");

            services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<CartCleanupJob>();
            services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
            {
                var catalogBaseUrl = configuration["Services:CatalogServiceBaseUrl"]
                    ?? throw new InvalidOperationException("Services:CatalogServiceBaseUrl is missing.");
                client.BaseAddress = new Uri(catalogBaseUrl);
            });

            services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
            {
                var inventoryBaseUrl = configuration["Services:InventoryServiceBaseUrl"]
                    ?? throw new InvalidOperationException("Services:InventoryServiceBaseUrl is missing.");
                client.BaseAddress = new Uri(inventoryBaseUrl);
            });
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
            services.AddMassTransit(busConfig =>
            {
                busConfig.AddConsumer<StockReservedEventConsumer>();
                busConfig.AddConsumer<StockUnavailableEventConsumer>();
                busConfig.AddConsumer<PaymentCompletedEventConsumer>();
                busConfig.AddConsumer<PaymentFailedEventConsumer>();

                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username(configuration["RabbitMq:Username"] ?? "guest");
                        h.Password(configuration["RabbitMq:Password"] ?? "guest");
                    });

                    cfg.ReceiveEndpoint("order-stock-reserved-queue", e =>
                    {
                        e.ConfigureConsumer<StockReservedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("order-stock-unavailable-queue", e =>
                    {
                        e.ConfigureConsumer<StockUnavailableEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("order-payment-completed-queue", e =>
                    {
                        e.ConfigureConsumer<PaymentCompletedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("order-payment-failed-queue", e =>
                    {
                        e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
                    });
                });
            });

            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddHostedService<OutboxPublisherService>();
            return services;
        }
    }
}
