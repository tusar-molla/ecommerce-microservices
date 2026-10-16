using CatalogService.Application.Interfaces;
using CatalogService.Infrastructure.Caching;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is missing.");

            services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IFileRepository, FileRepository>();

            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(configuration["Redis:ConnectionString"] ?? "localhost:6379");
                options.AbortOnConnectFail = false;   // start even if Redis is down
                options.ConnectTimeout = 2000;
                options.AsyncTimeout = 1000;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }
    }
}
