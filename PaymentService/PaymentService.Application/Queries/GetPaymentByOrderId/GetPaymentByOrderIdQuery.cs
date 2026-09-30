using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Queries.GetPaymentByOrderId
{
    public class GetPaymentByOrderIdQuery : IRequest<PaymentDto?>
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
    }

    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
