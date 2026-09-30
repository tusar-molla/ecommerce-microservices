using Dapper;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Infrastructure.Repositories
{
    public class IpnCallbackRepository : IIpnCallbackRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public IpnCallbackRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<bool> HasBeenProcessedAsync(string transactionId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "SELECT COUNT(1) FROM ProcessedIpnCallbacks WHERE TransactionId = @TransactionId";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { TransactionId = transactionId });
            return count > 0;
        }

        public async Task MarkAsProcessedAsync(string transactionId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "INSERT INTO ProcessedIpnCallbacks (TransactionId, ProcessedAt) VALUES (@TransactionId, @ProcessedAt)";
            await connection.ExecuteAsync(sql, new { TransactionId = transactionId, ProcessedAt = DateTime.UtcNow });
        }
    }
}
