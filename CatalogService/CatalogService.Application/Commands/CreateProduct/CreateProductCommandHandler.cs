using CatalogService.Application.Interfaces;
using CatalogService.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace CatalogService.Application.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFileStorageService _fileStorageService;

        public CreateProductCommandHandler(IProductRepository productRepository, IFileRepository fileRepository, IFileStorageService fileStorageService)
        {
            _productRepository = productRepository;
            _fileRepository = fileRepository;
            _fileStorageService = fileStorageService;
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
                IsActive = true
            };

            await _productRepository.CreateAsync(product);

            for (int i = 0; i < request.Images.Count; i++)
            {
                var image = request.Images[i];
                var fileUrl = await _fileStorageService.UploadAsync(image.FileStream, image.FileName, image.ContentType);

                var fileAsset = new FileAsset
                {
                    Id = Guid.NewGuid(),
                    EntityType = "Product",
                    EntityId = product.Id,
                    FileUrl = fileUrl,
                    FileName = image.FileName,
                    ContentType = image.ContentType,
                    IsPrimary = i == 0 // first uploaded image becomes primary by default
                };

                await _fileRepository.CreateAsync(fileAsset);
            }

            return product.Id;
        }
    }
}
