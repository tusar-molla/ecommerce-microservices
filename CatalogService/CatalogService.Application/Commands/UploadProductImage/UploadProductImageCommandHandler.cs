using CatalogService.Application.Interfaces;
using CatalogService.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Commands.UploadProductImage
{
    public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, string>
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFileStorageService _fileStorageService;


        public UploadProductImageCommandHandler(IProductRepository productRepository,IFileStorageService fileStorageService, IFileRepository fileRepository)
        {
            _productRepository = productRepository;
            _fileRepository = fileRepository;
            _fileStorageService = fileStorageService;
        }
        public async Task<string> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product is null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            var imageUrl = await _fileStorageService.UploadAsync(request.FileStream,request.FileName,request.ContentType);

            var existingFiles = await _fileRepository.GetByEntityAsync("Product", request.ProductId);
            var isFirstImage = !existingFiles.Any();

            var fileAsset = new FileAsset
            {
                Id = Guid.NewGuid(),
                EntityType = "Product",
                EntityId = request.ProductId,
                FileUrl = imageUrl,
                FileName = request.FileName,
                ContentType = request.ContentType,
                IsPrimary = isFirstImage // only auto-primary if this product had zero images before
            };

            await _fileRepository.CreateAsync(fileAsset);

            return imageUrl;
        }
    }
}
