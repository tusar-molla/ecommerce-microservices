using CatalogService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileRepository _fileRepository;

        public GetProductByIdQueryHandler(IProductRepository productRepository, IFileRepository fileRepository)
        {
            _productRepository = productRepository;
            _fileRepository = fileRepository;
        }
        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
            {
                return null;
            }

            var files = await _fileRepository.GetByEntityAsync("Product", product.Id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                Images = files.Select(f => new ProductImageDto
                {
                    Id = f.Id,
                    FileUrl = f.FileUrl,
                    IsPrimary = f.IsPrimary
                }).ToList()
            };
        }
    }
}
