using CatalogService.Application.Queries.GetProductById;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Queries.GetProductsByIds
{
    public class GetProductsByIdsQuery : IRequest<IEnumerable<ProductDto>>
    {
        public List<Guid> Ids { get; set; } = new();
    }
}
