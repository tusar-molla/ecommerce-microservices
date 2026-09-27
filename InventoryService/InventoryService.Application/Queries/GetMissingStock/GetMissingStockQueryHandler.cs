using InventoryService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Queries.GetMissingStock
{
    public class GetMissingStockQueryHandler : IRequestHandler<GetMissingStockQuery, IEnumerable<Guid>>
    {
        private readonly IProductCatalogClient _productCatalogClient;
        private readonly IStockRepository _stockRepository;

        public GetMissingStockQueryHandler(
            IProductCatalogClient productCatalogClient,
            IStockRepository stockRepository)
        {
            _productCatalogClient = productCatalogClient;
            _stockRepository = stockRepository;
        }

        public async Task<IEnumerable<Guid>> Handle(GetMissingStockQuery request, CancellationToken cancellationToken)
        {
            var allProductIds = (await _productCatalogClient.GetAllActiveProductIdsAsync()).ToList();
            var existingStockProductIds = (await _stockRepository.GetExistingProductIdsAsync(allProductIds)).ToHashSet();

            return allProductIds.Where(id => !existingStockProductIds.Contains(id));
        }
    }
}
