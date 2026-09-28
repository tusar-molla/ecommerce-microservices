using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace OrderService.Infrastructure.ExternalServices
{
    public class InventoryClient : IInventoryClient
    {
        private readonly HttpClient _httpClient;

        public InventoryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<StockLevelInfo>> GetStockLevelsAsync(IEnumerable<Guid> productIds)
        {
            var idList = productIds.ToList();
            if (idList.Count == 0)
            {
                return Enumerable.Empty<StockLevelInfo>();
            }

            var query = string.Join("&", idList.Select(id => $"productIds={id}"));
            var response = await _httpClient.GetAsync($"/api/stock-levels?{query}");

            if (!response.IsSuccessStatusCode)
            {
                return idList.Select(id => new StockLevelInfo { ProductId = id, QuantityAvailable = 0 });
            }

            var results = await response.Content.ReadFromJsonAsync<List<StockLevelResponse>>();

            return results?.Select(r => new StockLevelInfo
            {
                ProductId = r.ProductId,
                QuantityAvailable = r.QuantityAvailable
            }) ?? Enumerable.Empty<StockLevelInfo>();
        }

        private class StockLevelResponse
        {
            public Guid ProductId { get; set; }
            public int QuantityAvailable { get; set; }
        }
    }
}
