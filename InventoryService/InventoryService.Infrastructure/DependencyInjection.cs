using InventoryService.Application.EventHandlers;
using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.ExternalServices;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InventoryService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is missing.");

            services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IStockReservationRepository, StockReservationRepository>();

            services.AddMassTransit(busConfig =>
            {
                busConfig.AddConsumer<OrderPlacedEventConsumer>();
                busConfig.AddConsumer<StockReleaseRequestedEventConsumer>();

                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username(configuration["RabbitMq:Username"] ?? "guest");
                        h.Password(configuration["RabbitMq:Password"] ?? "guest");
                    });

                    cfg.ReceiveEndpoint("inventory-order-placed-queue", e =>
                    {
                        e.ConfigureConsumer<OrderPlacedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("inventory-stock-release-queue", e =>
                    {
                        e.ConfigureConsumer<StockReleaseRequestedEventConsumer>(context);
                    });
                });
            });

            services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
            {
                var catalogBaseUrl = configuration["Services:CatalogServiceBaseUrl"]
                    ?? throw new InvalidOperationException("Services:CatalogServiceBaseUrl is missing.");
                client.BaseAddress = new Uri(catalogBaseUrl);
            });

            return services;
        }
    }
}