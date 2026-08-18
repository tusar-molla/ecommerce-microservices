using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        public RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository,IUserRepository userRepository,ITokenService tokenService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (existingToken is null || !existingToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var user = await _userRepository.GetByIdAsync(existingToken.UserId);
            if (user is null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }
            // Revoke the old refresh token (rotation — prevents reuse)
            await _refreshTokenRepository.RevokeAsync(existingToken.Id);

            // Issue new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

            var newRefreshToken = new Models.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = newRefreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _refreshTokenRepository.CreateAsync(newRefreshToken);

            return new RefreshTokenResult
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue
            };
        }
    }
}
