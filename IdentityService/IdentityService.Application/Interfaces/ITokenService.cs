using IdentityService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
