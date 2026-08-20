using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType);
    }
}
