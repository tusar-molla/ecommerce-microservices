using IdentityService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.DTOs
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeAsync(Guid id);
    }
}
