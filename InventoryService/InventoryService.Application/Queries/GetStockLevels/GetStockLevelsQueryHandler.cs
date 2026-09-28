using InventoryService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Queries.GetStockLevels
{
    public class GetStockLevelsQueryHandler : IRequestHandler<GetStockLevelsQuery, IEnumerable<StockLevelDto>>
    {
        private readonly IStockRepository _stockRepository;

        public GetStockLevelsQueryHandler(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<IEnumerable<StockLevelDto>> Handle(GetStockLevelsQuery request, CancellationToken cancellationToken)
        {
            var stockRecords = (await _stockRepository.GetByProductIdsAsync(request.ProductIds))
                .ToDictionary(s => s.ProductId);

            return request.ProductIds.Select(id => new StockLevelDto
            {
                ProductId = id,
                QuantityAvailable = stockRecords.TryGetValue(id, out var stock) ? stock.QuantityAvailable : 0
            });
        }
    }
}
