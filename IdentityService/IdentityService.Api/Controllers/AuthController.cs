using IdentityService.Application.Commands.LoginUser;
using IdentityService.Application.Commands.Logout;
using IdentityService.Application.Commands.RefreshToken;
using IdentityService.Application.Commands.RegisterUser;
using IdentityService.Application.DTOs;
using IdentityService.Application.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public AuthController(IMediator mediator, IRefreshTokenRepository refreshTokenRepository)
        {
            _mediator = mediator;
            _refreshTokenRepository = refreshTokenRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var userId = await _mediator.Send(command);
            return Ok(new { UserId = userId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetCurrentUserQuery { UserId = userId });
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestBody body)
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
            var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

            var expiresAt = expClaim is not null
                ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime
                : DateTime.UtcNow;

            var logoutCommand = new LogoutCommand
            {
                RefreshToken = body.RefreshToken,
                Jti = jti,
                AccessTokenExpiresAt = expiresAt
            };

            await _mediator.Send(logoutCommand);
            return Ok(new { Message = "Logged out successfully." });
        }

        public class LogoutRequestBody
        {
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
}
