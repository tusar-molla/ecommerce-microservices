using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<RefreshTokenResult>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResult
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
