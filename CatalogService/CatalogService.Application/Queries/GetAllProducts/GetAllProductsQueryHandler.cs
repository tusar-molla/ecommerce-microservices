using CatalogService.Application.Interfaces;
using CatalogService.Application.Queries.GetProductById;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Queries.GetAllProducts
{
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ICacheService _cache;

        public GetAllProductsQueryHandler(IProductRepository productRepository, IFileRepository fileRepository, ICacheService cache)
        {
            _productRepository = productRepository;
            _fileRepository = fileRepository;
            _cache = cache;
        }

        public Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var key = $"products:p{pageNumber}:s{pageSize}:c{request.CategoryId?.ToString() ?? "all"}";

            return _cache.GetOrCreateAsync<PagedResult<ProductDto>>(CacheAreas.Catalog, key, async () =>
            {
                var (items, totalCount) = await _productRepository.GetPagedAsync(pageNumber, pageSize, request.CategoryId);
                var itemList = items.ToList();

                var allFiles = await _fileRepository.GetByEntityIdsAsync("Product", itemList.Select(p => p.Id).ToList());
                var filesByProduct = allFiles.GroupBy(f => f.EntityId).ToDictionary(g => g.Key, g => g.ToList());

                var dtos = itemList.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Sku = p.Sku,
                    CategoryId = p.CategoryId,
                    Images = filesByProduct.TryGetValue(p.Id, out var files)
                        ? files.Select(f => new ProductImageDto { Id = f.Id, FileUrl = f.FileUrl, IsPrimary = f.IsPrimary }).ToList()
                        : new List<ProductImageDto>()
                }).ToList();

                return new PagedResult<ProductDto>
                {
                    Items = dtos,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }, CacheAreas.DefaultTtl)!;
        }
    }
}
