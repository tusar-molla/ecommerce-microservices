using System.Text.Json;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379");
    options.AbortOnConnectFail = false;   // start even if Redis is down
    options.ConnectTimeout = 2000;
    options.AsyncTimeout = 1000;
    return ConnectionMultiplexer.Connect(options);
});

var app = builder.Build();

// Internal service-to-service endpoints must never be reachable from outside
app.Use(async (context, next) =>
{
    if (context.Request.Path.Value?.Contains("/internal/", StringComparison.OrdinalIgnoreCase) == true)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await next();
});

// Reject tokens that were logged out. The signature is still verified by each service,
// so a forged token cannot get through; this only adds the "revoked" check.
app.Use(async (context, next) =>
{
    var header = context.Request.Headers.Authorization.ToString();
    var jti = string.IsNullOrEmpty(header) ? null : ReadJti(header);

    if (jti is not null)
    {
        try
        {
            var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            if (redis.IsConnected && await redis.GetDatabase().KeyExistsAsync($"revoked-jti:{jti}"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
        catch (Exception ex)
        {
            // If Redis is unavailable, let the request through rather than block every user
            app.Logger.LogWarning(ex, "Revoked-token check skipped because Redis is unavailable");
        }
    }

    await next();
});

app.MapReverseProxy();

app.Run();

static string? ReadJti(string header)
{
    if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;

    var parts = header.Substring(7).Trim().Split('.');
    if (parts.Length != 3) return null;

    try
    {
        var payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
        return doc.RootElement.TryGetProperty("jti", out var jti) ? jti.GetString() : null;
    }
    catch
    {
        return null;
    }
}
