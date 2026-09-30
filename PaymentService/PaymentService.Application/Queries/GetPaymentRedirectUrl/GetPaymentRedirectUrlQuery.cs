using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Queries.GetPaymentRedirectUrl
{
    public class GetPaymentRedirectUrlQuery : IRequest<string?>
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
    }
}
