using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Models
{
    public enum PaymentStatus
    {
        Initiated,
        AwaitingGatewayRedirect,
        Completed,
        Failed,
        Cancelled
    }
}
