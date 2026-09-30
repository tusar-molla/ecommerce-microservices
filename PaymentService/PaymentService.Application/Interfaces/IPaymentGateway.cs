using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentGateway
    {
        Task<InitiateSessionResult> InitiateSessionAsync(InitiateSessionRequest request);
        Task<ValidationResult> ValidateTransactionAsync(string validationId);
    }
    public class InitiateSessionRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string CustomerPostcode { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
    }

    public class InitiateSessionResult
    {
        public bool Success { get; set; }
        public string GatewayPageUrl { get; set; } = string.Empty;
        public string SessionKey { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

}
