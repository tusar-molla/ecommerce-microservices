using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.EventHandlers;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.ExternalServices;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Repositories;
using System;

namespace NotificationService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is missing.");

            services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
            services.AddScoped<INotificationLogRepository, NotificationLogRepository>();
            services.AddScoped<IEmailSender, MailKitEmailSender>();


            services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
            {
                var url = configuration["Services:OrderServiceBaseUrl"]
                    ?? throw new InvalidOperationException("Services:OrderServiceBaseUrl is missing.");
                client.BaseAddress = new Uri(url);
            });

            services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
            {
                var url = configuration["Services:IdentityServiceBaseUrl"]
                    ?? throw new InvalidOperationException("Services:IdentityServiceBaseUrl is missing.");
                client.BaseAddress = new Uri(url);
            });

            services.AddMassTransit(busConfig =>
            {
                busConfig.AddConsumer<PaymentCompletedEventConsumer>();
                busConfig.AddConsumer<PaymentFailedEventConsumer>();
                busConfig.AddConsumer<StockUnavailableEventConsumer>();

                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
                    {
                        h.Username(configuration["RabbitMq:Username"] ?? "guest");
                        h.Password(configuration["RabbitMq:Password"] ?? "guest");
                    });

                    cfg.ReceiveEndpoint("notification-payment-completed-queue", e =>
                    {
                        e.ConfigureConsumer<PaymentCompletedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("notification-payment-failed-queue", e =>
                    {
                        e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("notification-stock-unavailable-queue", e =>
                    {
                        e.ConfigureConsumer<StockUnavailableEventConsumer>(context);
                    });
                });
            });
            return services;
        }
    }
}