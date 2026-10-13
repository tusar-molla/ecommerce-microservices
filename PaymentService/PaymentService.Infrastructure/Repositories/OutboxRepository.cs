using Dapper;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using PaymentService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Infrastructure.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OutboxRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT TOP (@BatchSize) Id, EventType, Payload, CreatedAt, ProcessedAt, Attempts, LastError
            FROM OutboxMessages
            WHERE ProcessedAt IS NULL
            ORDER BY CreatedAt";

            var rows = await connection.QueryAsync<OutboxMessage>(sql, new { BatchSize = batchSize });
            return rows.ToList();
        }

        public async Task MarkProcessedAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE OutboxMessages SET ProcessedAt = @Now, LastError = NULL WHERE Id = @Id",
                new { Id = id, Now = DateTime.UtcNow });
        }

        public async Task RecordFailureAsync(Guid id, string error)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE OutboxMessages SET Attempts = Attempts + 1, LastError = @Error WHERE Id = @Id",
                new { Id = id, Error = Truncate(error) });
        }

        public async Task MarkDeadAsync(Guid id, string error)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "UPDATE OutboxMessages SET Attempts = Attempts + 1, ProcessedAt = @Now, LastError = @Error WHERE Id = @Id",
                new { Id = id, Now = DateTime.UtcNow, Error = Truncate("DEAD: " + error) });
        }

        private static string Truncate(string value) => value.Length <= 1000 ? value : value[..1000];
    }
}
