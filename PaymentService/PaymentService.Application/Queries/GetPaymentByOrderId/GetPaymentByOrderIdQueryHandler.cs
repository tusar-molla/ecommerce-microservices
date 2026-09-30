using MediatR;
using PaymentService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Queries.GetPaymentByOrderId
{
    public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto?>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentByOrderIdQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);

            if (payment is null || payment.UserId != request.UserId)
            {
                return null;
            }

            return new PaymentDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status.ToString(),
                FailureReason = payment.FailureReason,
                CreatedAt = payment.CreatedAt
            };
        }
    }
}
