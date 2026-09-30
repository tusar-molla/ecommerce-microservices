using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.EventHandlers;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.ExternalServices;
using PaymentService.Infrastructure.Gateway;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Repositories;
using System;

namespace PaymentService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is missing.");

            services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IIpnCallbackRepository, IpnCallbackRepository>();

            services.AddHttpClient<IPaymentGateway, SslCommerzPaymentGateway>(client =>
            {
                var baseUrl = configuration["SslCommerz:SandboxBaseUrl"]
                    ?? throw new InvalidOperationException("SslCommerz:SandboxBaseUrl is missing.");
                client.BaseAddress = new Uri(baseUrl);
            });

            services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
            {
                var orderServiceUrl = configuration["Services:OrderServiceBaseUrl"]
                    ?? throw new InvalidOperationException("Services:OrderServiceBaseUrl is missing.");
                client.BaseAddress = new Uri(orderServiceUrl);
            });

            services.AddHttpClient<IIdentityServiceClient, IdentityServiceClient>(client =>
            {
                var identityServiceUrl = configuration["Services:IdentityServiceBaseUrl"]
                    ?? throw new InvalidOperationException("Services:IdentityServiceBaseUrl is missing.");
                client.BaseAddress = new Uri(identityServiceUrl);
            });

            services.AddMassTransit(busConfig =>
            {
                busConfig.AddConsumer<StockReservedEventConsumer>();

                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("payment-stock-reserved-queue", e =>
                    {
                        e.ConfigureConsumer<StockReservedEventConsumer>(context);
                    });
                });
            });
            return services;
        }
    }
}