using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace OrderService.Infrastructure.ExternalServices
{
    public class ProductCatalogClient : IProductCatalogClient
    {
        private readonly HttpClient _httpClient;

        public ProductCatalogClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<IEnumerable<ProductCatalogInfo>> GetProductsByIdsAsync(IEnumerable<Guid> productIds)
        {
            var idList = productIds.ToList();
            if (idList.Count == 0)
            {
                return Enumerable.Empty<ProductCatalogInfo>();
            }

            var query = string.Join("&", idList.Select(id => $"ids={id}"));
            var response = await _httpClient.GetAsync($"/api/products/bulk?{query}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch products from Catalog Service. Status: {response.StatusCode}");
            }

            var catalogProducts = await response.Content.ReadFromJsonAsync<List<CatalogProductResponse>>();

            return catalogProducts?.Select(p => new ProductCatalogInfo
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.FileUrl
                    ?? p.Images.FirstOrDefault()?.FileUrl
                    ?? string.Empty
            }) ?? Enumerable.Empty<ProductCatalogInfo>();
        }
        private class CatalogProductResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public List<CatalogImageResponse> Images { get; set; } = new();
        }

        private class CatalogImageResponse
        {
            public string FileUrl { get; set; } = string.Empty;
            public bool IsPrimary { get; set; }
        }
    }
}
