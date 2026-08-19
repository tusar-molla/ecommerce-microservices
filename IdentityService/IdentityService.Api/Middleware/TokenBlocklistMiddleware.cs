using IdentityService.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityService.Api.Middleware
{
    public class TokenBlocklistMiddleware
    {
        private readonly RequestDelegate _next;
        public TokenBlocklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, ITokenBlocklistService blocklistService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (!string.IsNullOrEmpty(jti))
                {
                    var isRevoked = await blocklistService.IsRevokedAsync(jti);
                    if (isRevoked)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            StatusCode = 401,
                            Message = "This token has been revoked. Please log in again."
                        });
                        return; // short-circuit — do NOT call _next(context)
                    }
                }
            }

            await _next(context);
        }
    }
}
