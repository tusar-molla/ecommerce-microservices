using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Commands.UploadProductImage
{
    public class UploadProductImageCommand : IRequest<string>
    {
        public Guid ProductId { get; set; }
        public Stream FileStream { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
