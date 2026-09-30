using MediatR;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries.GetPaymentByOrderId;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Queries.GetMyPayments
{
    public class GetMyPaymentsQueryHandler : IRequestHandler<GetMyPaymentsQuery, IEnumerable<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetMyPaymentsQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<PaymentDto>> Handle(GetMyPaymentsQuery request, CancellationToken cancellationToken)
        {
            var payments = await _paymentRepository.GetByUserIdAsync(request.UserId);

            return payments.Select(p => new PaymentDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                FailureReason = p.FailureReason,
                CreatedAt = p.CreatedAt
            });
        }
    }
}
