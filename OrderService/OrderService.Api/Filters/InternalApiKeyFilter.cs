using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OrderService.Api.Filters
{
    public class InternalApiKeyFilter : IAsyncActionFilter
    {
        private readonly IConfiguration _configuration;

        public InternalApiKeyFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var expectedKey = _configuration["InternalApiKey"];
            var providedKey = context.HttpContext.Request.Headers["X-Internal-Api-Key"].FirstOrDefault();

            if (string.IsNullOrEmpty(expectedKey) || providedKey != expectedKey)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
