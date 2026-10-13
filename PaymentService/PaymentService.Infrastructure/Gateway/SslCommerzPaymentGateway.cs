using Microsoft.Extensions.Configuration;
using PaymentService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;

namespace PaymentService.Infrastructure.Gateway
{
    public class SslCommerzPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _storeId;
        private readonly string _storePassword;

        public SslCommerzPaymentGateway(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _storeId = configuration["SslCommerz:StoreId"]
                ?? throw new InvalidOperationException("SslCommerz:StoreId is missing.");
            _storePassword = configuration["SslCommerz:StorePassword"]
                ?? throw new InvalidOperationException("SslCommerz:StorePassword is missing.");
        }

        public async Task<InitiateSessionResult> InitiateSessionAsync(InitiateSessionRequest request)
        {
            var formData = new Dictionary<string, string>
            {
                ["store_id"] = _storeId,
                ["store_passwd"] = _storePassword,
                ["total_amount"] = request.Amount.ToString("F2"),
                ["currency"] = request.Currency,
                ["tran_id"] = request.TransactionId,
                ["success_url"] = request.SuccessUrl,
                ["fail_url"] = request.FailUrl,
                ["cancel_url"] = request.CancelUrl,
                ["ipn_url"] = request.IpnUrl,
                ["cus_name"] = request.CustomerName,
                ["cus_email"] = request.CustomerEmail,
                ["cus_add1"] = string.IsNullOrWhiteSpace(request.CustomerAddress) ? "N/A" : request.CustomerAddress,
                ["cus_city"] = string.IsNullOrWhiteSpace(request.CustomerCity) ? "Dhaka" : request.CustomerCity,
                ["cus_postcode"] = string.IsNullOrWhiteSpace(request.CustomerPostcode) ? "1200" : request.CustomerPostcode,
                ["cus_country"] = "Bangladesh",
                ["cus_phone"] = string.IsNullOrWhiteSpace(request.CustomerPhone) ? "01700000000" : request.CustomerPhone,
                ["shipping_method"] = "NO",
                ["product_name"] = "Order Payment",
                ["product_category"] = "General",
                ["product_profile"] = "general"
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync("/gwprocess/v4/api.php", content);

            if (!response.IsSuccessStatusCode)
            {
                return new InitiateSessionResult { Success = false, FailureReason = $"Gateway returned {response.StatusCode}" };
            }

            var result = await response.Content.ReadFromJsonAsync<SslCommerzInitResponse>();

            if (result is null || result.status != "SUCCESS")
            {
                return new InitiateSessionResult
                {
                    Success = false,
                    FailureReason = result?.failedreason ?? "Unknown gateway error"
                };
            }

            return new InitiateSessionResult
            {
                Success = true,
                GatewayPageUrl = result.GatewayPageURL,
                SessionKey = result.sessionkey
            };
        }

        public async Task<ValidationResult> ValidateTransactionAsync(string validationId)
        {
            var query = $"?val_id={Uri.EscapeDataString(validationId)}" +
                        $"&store_id={Uri.EscapeDataString(_storeId)}" +
                        $"&store_passwd={Uri.EscapeDataString(_storePassword)}&format=json";

            var response = await _httpClient.GetAsync($"/validator/api/validationserverAPI.php{query}");

            // A network or server problem is not a verdict on the payment.
            // Throwing makes the IPN request fail, so the gateway can retry it later.
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SslCommerzValidationResponse>()
                ?? throw new InvalidOperationException("Empty response from the SSLCommerz validation API.");

            return new ValidationResult
            {
                IsValid = result.status is "VALID" or "VALIDATED",
                Status = result.status,
                Amount = decimal.TryParse(result.amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amt) ? amt : 0,
                Currency = result.currency
            };
        }

        private class SslCommerzValidationResponse
        {
            public string status { get; set; } = string.Empty;
            public string amount { get; set; } = string.Empty;
            public string currency { get; set; } = string.Empty;
        }

        private class SslCommerzInitResponse
        {
            public string status { get; set; } = string.Empty;
            public string GatewayPageURL { get; set; } = string.Empty;
            public string sessionkey { get; set; } = string.Empty;
            public string? failedreason { get; set; }
        }
    }
}
