using MediatR;
using PaymentService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Queries.GetPaymentRedirectUrl
{
    public class GetPaymentRedirectUrlQueryHandler : IRequestHandler<GetPaymentRedirectUrlQuery, string?>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentRedirectUrlQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<string?> Handle(GetPaymentRedirectUrlQuery request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId);

            if (payment is null || payment.UserId != request.UserId)
            {
                return null;
            }

            return payment.GatewayPageUrl;
        }
    }
}
