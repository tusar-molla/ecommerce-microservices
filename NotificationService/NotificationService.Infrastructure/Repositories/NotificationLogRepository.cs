using Dapper;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using NotificationService.Infrastructure.Persistence;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Infrastructure.Repositories
{
    public class NotificationLogRepository : INotificationLogRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public NotificationLogRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(NotificationLog log)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO NotificationLogs (Id, OrderId, RecipientEmail, NotificationType, Subject, Status, CreatedAt)
            VALUES (@Id, @OrderId, @RecipientEmail, @NotificationType, @Subject, @Status, @CreatedAt)";

            await connection.ExecuteAsync(sql, new
            {
                log.Id,
                log.OrderId,
                log.RecipientEmail,
                log.NotificationType,
                log.Subject,
                Status = log.Status.ToString(),
                log.CreatedAt
            });

            return log.Id;
        }

        public async Task UpdateStatusAsync(Guid id, NotificationStatus status, string? errorMessage)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            UPDATE NotificationLogs
            SET Status = @Status, ErrorMessage = @ErrorMessage, SentAt = @SentAt
            WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Status = status.ToString(),
                ErrorMessage = errorMessage,
                SentAt = status == NotificationStatus.Sent ? DateTime.UtcNow : (DateTime?)null
            });
        }
    }
}
