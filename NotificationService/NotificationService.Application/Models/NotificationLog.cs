using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Application.Models
{
    public class NotificationLog
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}
