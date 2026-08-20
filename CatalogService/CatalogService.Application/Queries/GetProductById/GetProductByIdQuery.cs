using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ProductDto?>
    {
        public Guid Id { get; set; }
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Sku { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
    }

    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
