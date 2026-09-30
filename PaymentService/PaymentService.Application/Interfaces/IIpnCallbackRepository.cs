using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Interfaces
{

    public interface IIpnCallbackRepository
    {
        Task<bool> HasBeenProcessedAsync(string transactionId);
        Task MarkAsProcessedAsync(string transactionId);
    }
}
