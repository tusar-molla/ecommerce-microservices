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

        public GetAllProductsQueryHandler(IProductRepository productRepository, IFileRepository fileRepository)
        {
            _productRepository = productRepository;
            _fileRepository = fileRepository;
        }

        public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _productRepository.GetPagedAsync(
                request.PageNumber, request.PageSize, request.CategoryId);

            var productIds = items.Select(p => p.Id).ToList();
            var allFiles = await _fileRepository.GetByEntityIdsAsync("Product", productIds);
            var filesByProduct = allFiles.GroupBy(f => f.EntityId).ToDictionary(g => g.Key, g => g.ToList());

            var dtos = items.Select(p => new ProductDto
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
            });

            return new PagedResult<ProductDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
