using CatalogService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<Guid> CreateAsync(Product product);
        Task<Product?> GetByIdAsync(Guid id);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId);
        Task<bool> SkuExistsAsync(string sku);
        Task UpdateAsync(Product product);
        Task SoftDeleteAsync(Guid id);
        Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}
