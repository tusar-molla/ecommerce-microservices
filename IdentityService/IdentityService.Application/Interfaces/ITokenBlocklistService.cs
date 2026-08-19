using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Interfaces
{
    public interface ITokenBlocklistService
    {
        Task RevokeAsync(string jti, DateTime tokenExpiresAt);
        Task<bool> IsRevokedAsync(string jti);
    }
}
