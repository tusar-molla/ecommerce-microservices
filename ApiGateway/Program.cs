var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

app.MapReverseProxy();

app.Run();
