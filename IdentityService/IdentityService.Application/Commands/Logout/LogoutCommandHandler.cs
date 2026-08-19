using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenBlocklistService _tokenBlocklistService;

        public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository, ITokenBlocklistService tokenBlocklistService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _tokenBlocklistService = tokenBlocklistService;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (token is not null)
            {
                await _refreshTokenRepository.RevokeAsync(token.Id);
            }

            if (!string.IsNullOrEmpty(request.Jti))
            {
                await _tokenBlocklistService.RevokeAsync(request.Jti, request.AccessTokenExpiresAt);
            }

            return Unit.Value;
        }
    }
}
