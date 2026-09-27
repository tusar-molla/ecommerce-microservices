using InventoryService.Application.Commands.SetInitialStock;
using InventoryService.Application.Interfaces;
using InventoryService.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Commands.SetInitialStock
{
    public class SetInitialStockCommandHandler : IRequestHandler<SetInitialStockCommand, Guid>
    {
        private readonly IStockRepository _stockRepository;

        public SetInitialStockCommandHandler(IStockRepository stockRepository)
        {
            _stockRepository = stockRepository;
        }

        public async Task<Guid> Handle(SetInitialStockCommand request, CancellationToken cancellationToken)
        {
            var existingStock = await _stockRepository.GetByProductIdAsync(request.ProductId);
            if (existingStock is not null)
            {
                throw new InvalidOperationException(
                    $"Stock already exists for this product. Use the adjust-stock endpoint to change quantity instead.");
            }

            var stock = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                QuantityAvailable = request.InitialQuantity,
                QuantityReserved = 0,
                UpdatedAt = DateTime.UtcNow
            };

            return await _stockRepository.CreateAsync(stock);
        }
    }
}
