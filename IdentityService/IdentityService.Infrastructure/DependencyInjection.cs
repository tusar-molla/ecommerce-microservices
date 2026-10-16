using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is missing.");

            services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ITokenBlocklistService, SqlTokenBlocklistService>(); services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(configuration["Redis:ConnectionString"] ?? "localhost:6379");
                options.AbortOnConnectFail = false;   // start even if Redis is down
                options.ConnectTimeout = 2000;
                options.AsyncTimeout = 1000;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddScoped<SqlTokenBlocklistService>();
            services.AddScoped<ITokenBlocklistService, RedisTokenBlocklistService>();

            var jwtSecretKey = configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");
            var jwtIssuer = configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
            var jwtAudience = configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience is missing.");
            var jwtExpiryMinutes = int.Parse(configuration["Jwt:AccessTokenExpiryMinutes"] ?? "60");

            services.AddSingleton<ITokenService>(new TokenService(jwtSecretKey, jwtIssuer, jwtAudience, jwtExpiryMinutes));

            return services;
        }
    }
}
