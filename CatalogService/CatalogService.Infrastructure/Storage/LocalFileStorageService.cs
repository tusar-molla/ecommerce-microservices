using CatalogService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _webRootPath;
        private readonly string _baseUrl;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
            _baseUrl = configuration["App:BaseUrl"] ?? "https://localhost:7179";

            if (!Directory.Exists(_webRootPath))
            {
                Directory.CreateDirectory(_webRootPath);
            }
        }
        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_webRootPath, uniqueFileName);

            using (var fileOutputStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileOutputStream);
            }

            return $"{_baseUrl}/images/products/{uniqueFileName}";
        }

    }
}
