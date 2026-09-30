using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;
        public string TransactionId { get; set; } = string.Empty;
        public string? GatewaySessionKey { get; set; }
        public string? GatewayValidationId { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? GatewayPageUrl { get; set; }
    }
}
