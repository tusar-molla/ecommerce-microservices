using IdentityService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(User user);
    }
}
