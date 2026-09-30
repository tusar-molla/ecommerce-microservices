using Microsoft.Extensions.Configuration;
using PaymentService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace PaymentService.Infrastructure.ExternalServices
{
    public class OrderServiceClient : IOrderServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _internalApiKey;

        public OrderServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _internalApiKey = configuration["InternalApiKey"]
                ?? throw new InvalidOperationException("InternalApiKey is missing.");
        }

        public async Task<OrderInfo?> GetOrderAsync(Guid orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/internal/{orderId}");
            request.Headers.Add("X-Internal-Api-Key", _internalApiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
            if (order is null)
            {
                return null;
            }

            return new OrderInfo
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status
            };
        }

        private class OrderResponse
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}
