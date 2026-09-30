using MediatR;
using PaymentService.Application.Queries.GetPaymentByOrderId;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Queries.GetMyPayments
{
    public class GetMyPaymentsQuery : IRequest<IEnumerable<PaymentDto>>
    {
        public Guid UserId { get; set; }
    }
}
