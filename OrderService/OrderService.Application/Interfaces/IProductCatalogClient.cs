using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Interfaces
{
    public interface IProductCatalogClient
    {
        Task<IEnumerable<ProductCatalogInfo>> GetProductsByIdsAsync(IEnumerable<Guid> productIds);
    }
    public class ProductCatalogInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
    //Step 2
}
