using InventoryService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace InventoryService.Infrastructure.ExternalServices
{
    public class ProductCatalogClient : IProductCatalogClient
    {
        private readonly HttpClient _httpClient;

        public ProductCatalogClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Guid>> GetAllActiveProductIdsAsync()
        {
            var response = await _httpClient.GetAsync("/api/products/ids");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch product ids from Catalog Service. Status: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<List<Guid>>() ?? new List<Guid>();
        }
    }
    }
