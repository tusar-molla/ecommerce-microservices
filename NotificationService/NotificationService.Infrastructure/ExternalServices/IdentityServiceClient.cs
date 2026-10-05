using Microsoft.Extensions.Configuration;
using NotificationService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace NotificationService.Infrastructure.ExternalServices
{
    public class IdentityServiceClient : IIdentityServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _internalApiKey;

        public IdentityServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _internalApiKey = configuration["InternalApiKey"]
                ?? throw new InvalidOperationException("InternalApiKey is missing.");
        }

        public async Task<UserInfo?> GetUserAsync(Guid userId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/auth/internal/{userId}");
            request.Headers.Add("X-Internal-Api-Key", _internalApiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserInfo>();
        }
    }
}
