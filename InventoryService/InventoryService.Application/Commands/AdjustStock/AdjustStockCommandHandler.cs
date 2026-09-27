using InventoryService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Commands.AdjustStock
{
    public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, Unit>
    {
        private readonly IStockRepository _stockRepository;

        public AdjustStockCommandHandler(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<Unit> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
        {
            var stock = await _stockRepository.GetByProductIdAsync(request.ProductId);
            if (stock is null)
            {
                throw new InvalidOperationException(
                    "No stock record exists for this product yet. Use the set-initial-stock endpoint first.");
            }

            var newQuantity = stock.QuantityAvailable + request.QuantityChange;
            if (newQuantity < 0)
            {
                throw new InvalidOperationException("Adjustment would result in negative available stock.");
            }

            stock.QuantityAvailable = newQuantity;
            stock.UpdatedAt = DateTime.UtcNow;

            await _stockRepository.UpdateAsync(stock);

            return Unit.Value;
        }
    }
}
