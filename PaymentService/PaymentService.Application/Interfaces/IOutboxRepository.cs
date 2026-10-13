using PaymentService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize);
        Task MarkProcessedAsync(Guid id);
        Task RecordFailureAsync(Guid id, string error);
        Task MarkDeadAsync(Guid id, string error);
    }
}
