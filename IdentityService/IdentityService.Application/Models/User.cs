using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public enum UserRole
    {
        Customer,
        Admin
    }

}
