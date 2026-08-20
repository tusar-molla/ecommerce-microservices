using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Sku { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public List<UploadedFileData> Images { get; set; } = new();
    }

    public class UploadedFileData
    {
        public Stream FileStream { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
