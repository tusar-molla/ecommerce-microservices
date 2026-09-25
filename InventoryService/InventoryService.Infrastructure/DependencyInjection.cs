using InventoryService.Application.EventHandlers;
using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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

                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("inventory-order-placed-queue", e =>
                    {
                        e.ConfigureConsumer<OrderPlacedEventConsumer>(context);
                    });
                });
            });

            return services;
        }
    }
}
