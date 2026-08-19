using CatalogService.Application.Interfaces;
using CatalogService.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;

        public CreateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var skuExists = await _productRepository.SkuExistsAsync(request.Sku);
            if (skuExists)
            {
                throw new InvalidOperationException($"A product with SKU '{request.Sku}' already exists.");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Sku = request.Sku,
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl,
                IsActive = true
            };

            return await _productRepository.CreateAsync(product);
        }
    }
}
