using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Commands.ProcessIpnCallback
{
    public class ProcessIpnCallbackCommand : IRequest<Unit>
    {
        public string TransactionId { get; set; } = string.Empty;
        public string ValidationId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
