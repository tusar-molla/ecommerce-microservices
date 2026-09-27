using CatalogService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Queries.GetAllProductIds
{
    public class GetAllProductIdsQueryHandler : IRequestHandler<GetAllProductIdsQuery, IEnumerable<Guid>>
    {
        private readonly IProductRepository _productRepository;

        public GetAllProductIdsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Guid>> Handle(GetAllProductIdsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetAllIdsAsync();
            return products;
        }
    }
}
