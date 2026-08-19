using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands.Logout
{
    public class LogoutCommand : IRequest<Unit>
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string Jti { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}