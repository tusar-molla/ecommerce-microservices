using NotificationService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationLogRepository
    {
        Task<Guid> CreateAsync(NotificationLog log);
        Task UpdateStatusAsync(Guid id, NotificationStatus status, string? errorMessage);
        Task<bool> HasBeenSentAsync(Guid orderId, string notificationType);
    }
}
