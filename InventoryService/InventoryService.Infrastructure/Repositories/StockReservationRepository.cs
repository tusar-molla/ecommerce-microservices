using Dapper;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using InventoryService.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Infrastructure.Repositories
{
    public class StockReservationRepository : IStockReservationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public StockReservationRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(StockReservation reservation)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO StockReservations (Id, OrderId, ProductId, Quantity, Status, CreatedAt)
            VALUES (@Id, @OrderId, @ProductId, @Quantity, @Status, @CreatedAt)";

            await connection.ExecuteAsync(sql, new
            {
                reservation.Id,
                reservation.OrderId,
                reservation.ProductId,
                reservation.Quantity,
                Status = reservation.Status.ToString(),
                reservation.CreatedAt
            });

            return reservation.Id;
        }

        public async Task<IEnumerable<StockReservation>> GetByOrderIdAsync(Guid orderId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, OrderId, ProductId, Quantity, Status, CreatedAt
            FROM StockReservations
            WHERE OrderId = @OrderId";

            return await connection.QueryAsync<StockReservation>(sql, new { OrderId = orderId });
        }

        public async Task<ReservationResult> ReserveForOrderAsync(Guid messageId, string consumer, Guid orderId, IReadOnlyList<ReservationItem> items)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Claim the message. A repeat delivery hits the primary key.
                try
                {
                    await connection.ExecuteAsync(
                        "INSERT INTO ProcessedMessages (MessageId, Consumer) VALUES (@MessageId, @Consumer)",
                        new { MessageId = messageId, Consumer = consumer }, transaction);
                }
                catch (SqlException ex) when (ex.Number is 2627 or 2601)
                {
                    transaction.Rollback();
                    return await GetRecordedResultAsync(connection, messageId, consumer);
                }

                // 2. A savepoint lets us undo the stock changes below while keeping the claim above
                await connection.ExecuteAsync("SAVE TRANSACTION ReserveStock", transaction: transaction);

                var unavailable = new List<Guid>();
                foreach (var item in items)
                {
                    var affected = await connection.ExecuteAsync(@"
                UPDATE Stock
                SET QuantityAvailable = QuantityAvailable - @Quantity,
                    QuantityReserved  = QuantityReserved  + @Quantity,
                    UpdatedAt = @Now
                WHERE ProductId = @ProductId AND QuantityAvailable >= @Quantity",
                        new { item.ProductId, item.Quantity, Now = DateTime.UtcNow }, transaction);

                    if (affected == 0)
                    {
                        unavailable.Add(item.ProductId);
                    }
                }

                if (unavailable.Count > 0)
                {
                    await connection.ExecuteAsync("ROLLBACK TRANSACTION ReserveStock", transaction: transaction);
                    await RecordOutcomeAsync(connection, transaction, messageId, consumer,
                        "Unavailable", string.Join(",", unavailable));
                    transaction.Commit();

                    return new ReservationResult
                    {
                        Outcome = ReservationOutcome.Unavailable,
                        UnavailableProductIds = unavailable
                    };
                }

                foreach (var item in items)
                {
                    await connection.ExecuteAsync(@"
                INSERT INTO StockReservations (Id, OrderId, ProductId, Quantity, Status, CreatedAt)
                VALUES (@Id, @OrderId, @ProductId, @Quantity, 'Reserved', @Now)",
                        new { Id = Guid.NewGuid(), OrderId = orderId, item.ProductId, item.Quantity, Now = DateTime.UtcNow },
                        transaction);
                }

                await RecordOutcomeAsync(connection, transaction, messageId, consumer, "Reserved", null);
                transaction.Commit();

                return new ReservationResult { Outcome = ReservationOutcome.Reserved };
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static Task RecordOutcomeAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction transaction,Guid messageId, string consumer, string outcome, string? details)
        {
            return connection.ExecuteAsync(@"
        UPDATE ProcessedMessages SET Outcome = @Outcome, Details = @Details
        WHERE MessageId = @MessageId AND Consumer = @Consumer",
                new { MessageId = messageId, Consumer = consumer, Outcome = outcome, Details = details }, transaction);
        }

        private static async Task<ReservationResult> GetRecordedResultAsync(System.Data.IDbConnection connection, Guid messageId, string consumer)
        {
            var row = await connection.QuerySingleOrDefaultAsync<(string? Outcome, string? Details)>(
                "SELECT Outcome, Details FROM ProcessedMessages WHERE MessageId = @MessageId AND Consumer = @Consumer",
                new { MessageId = messageId, Consumer = consumer });

            var unavailable = (row.Details ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(Guid.Parse)
                .ToList();

            return new ReservationResult
            {
                IsDuplicate = true,
                Outcome = row.Outcome == "Unavailable" ? ReservationOutcome.Unavailable : ReservationOutcome.Reserved,
                UnavailableProductIds = unavailable
            };
        }
    }
}
