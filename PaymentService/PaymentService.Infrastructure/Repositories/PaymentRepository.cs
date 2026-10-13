using Dapper;
using Microsoft.Data.SqlClient;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using PaymentService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PaymentRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(Payment payment)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO Payments (Id, OrderId, UserId, Amount, Currency, Status, TransactionId,
                                   GatewaySessionKey, GatewayValidationId, FailureReason, CreatedAt,GatewayPageUrl)
            VALUES (@Id, @OrderId, @UserId, @Amount, @Currency, @Status, @TransactionId,
                    @GatewaySessionKey, @GatewayValidationId, @FailureReason, @CreatedAt, @GatewayPageUrl)";

            await connection.ExecuteAsync(sql, new
            {
                payment.Id,
                payment.OrderId,
                payment.UserId,
                payment.Amount,
                payment.Currency,
                Status = payment.Status.ToString(),
                payment.TransactionId,
                payment.GatewaySessionKey,
                payment.GatewayValidationId,
                payment.FailureReason,
                payment.CreatedAt,
                payment.GatewayPageUrl

            });

            return payment.Id;
        }

        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, OrderId, UserId, Amount, Currency, Status, TransactionId,
                   GatewaySessionKey, GatewayValidationId, FailureReason,GatewayPageUrl, CreatedAt, UpdatedAt
            FROM Payments
            WHERE Id = @Id";

            return await connection.QuerySingleOrDefaultAsync<Payment>(sql, new { Id = id });
        }

        public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT TOP 1 Id, OrderId, UserId, Amount, Currency, Status, TransactionId,
                   GatewaySessionKey, GatewayValidationId, FailureReason,GatewayPageUrl, CreatedAt, UpdatedAt
            FROM Payments
            WHERE OrderId = @OrderId
            ORDER BY CreatedAt DESC";

            return await connection.QuerySingleOrDefaultAsync<Payment>(sql, new { OrderId = orderId });
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, OrderId, UserId, Amount, Currency, Status, TransactionId,
                   GatewaySessionKey, GatewayValidationId, FailureReason,GatewayPageUrl, CreatedAt, UpdatedAt
            FROM Payments
            WHERE TransactionId = @TransactionId";

            return await connection.QuerySingleOrDefaultAsync<Payment>(sql, new { TransactionId = transactionId });
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, OrderId, UserId, Amount, Currency, Status, TransactionId,
                   GatewaySessionKey, GatewayValidationId, FailureReason,GatewayPageUrl,CreatedAt, UpdatedAt
            FROM Payments
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC";

            return await connection.QueryAsync<Payment>(sql, new { UserId = userId });
        }

        public async Task UpdateAsync(Payment payment)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            UPDATE Payments
            SET Status = @Status,
                GatewaySessionKey = @GatewaySessionKey,
                GatewayValidationId = @GatewayValidationId,
                FailureReason = @FailureReason,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                payment.Id,
                Status = payment.Status.ToString(),
                payment.GatewaySessionKey,
                payment.GatewayValidationId,
                payment.FailureReason,
                UpdatedAt = DateTime.UtcNow
            });
        }

        public async Task<bool> CompleteFromIpnAsync(Payment payment, string transactionId, OutboxMessage outboxMessage)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                try
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO ProcessedIpnCallbacks (TransactionId, ProcessedAt) VALUES (@TransactionId, @Now)",
                        new { TransactionId = transactionId, Now = DateTime.UtcNow }, transaction);
                }
                catch (SqlException ex) when (ex.Number is 2627 or 2601)
                {
                    transaction.Rollback();
                    return false; // this IPN was already processed
                }

                await connection.ExecuteAsync(@"
            UPDATE Payments
            SET Status = @Status, GatewayValidationId = @GatewayValidationId,
                FailureReason = @FailureReason, UpdatedAt = @Now
            WHERE Id = @Id",
                    new
                    {
                        payment.Id,
                        Status = payment.Status.ToString(),
                        payment.GatewayValidationId,
                        payment.FailureReason,
                        Now = DateTime.UtcNow
                    }, transaction);

                await connection.ExecuteAsync(@"
            INSERT INTO OutboxMessages (Id, EventType, Payload, CreatedAt)
            VALUES (@Id, @EventType, @Payload, @CreatedAt)",
                    new { outboxMessage.Id, outboxMessage.EventType, outboxMessage.Payload, outboxMessage.CreatedAt },
                    transaction);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
