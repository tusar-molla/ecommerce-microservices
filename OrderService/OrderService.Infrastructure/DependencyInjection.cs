using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Interfaces;
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

            return services;
        }
    }
}
