using PaymentService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Guid> CreateAsync(Payment payment);
        Task<Payment?> GetByIdAsync(Guid id);
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(Payment payment);
    }
}
