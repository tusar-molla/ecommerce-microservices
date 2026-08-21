using CatalogService.Application.Interfaces;
using CatalogService.Application.Queries.GetProductById;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Queries.GetProductsByIds
{
    public class GetProductsByIdsQueryHandler : IRequestHandler<GetProductsByIdsQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileRepository _fileRepository;

        public GetProductsByIdsQueryHandler(IProductRepository productRepository, IFileRepository fileRepository)
        {
            _productRepository = productRepository;
            _fileRepository = fileRepository;
        }
        public async Task<IEnumerable<ProductDto>> Handle(GetProductsByIdsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetByIdsAsync(request.Ids);
            var productList = products.ToList();

            var productIds = productList.Select(p => p.Id).ToList();
            var allFiles = await _fileRepository.GetByEntityIdsAsync("Product", productIds);
            var filesByProduct = allFiles.GroupBy(f => f.EntityId).ToDictionary(g => g.Key, g => g.ToList());

            return productList.Select(p => new ProductDto
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
        }
    }
}
