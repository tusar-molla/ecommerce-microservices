using MediatR;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Queries.GetCart
{
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductCatalogClient _productCatalogClient;
        private readonly IInventoryClient _inventoryClient;

        public GetCartQueryHandler(ICartRepository cartRepository, IProductCatalogClient productCatalogClient, IInventoryClient inventoryClient)
        {
            _cartRepository = cartRepository;
            _productCatalogClient = productCatalogClient;
            _inventoryClient = inventoryClient;
        }

        public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);

            if (cart is null)
            {
                return new CartDto { CartId = Guid.Empty, Items = new List<CartItemDto>() };
            }

            var cartItems = (await _cartRepository.GetItemsAsync(cart.Id)).ToList();

            if (cartItems.Count == 0)
            {
                return new CartDto { CartId = cart.Id, Items = new List<CartItemDto>() };
            }

            var productIds = cartItems.Select(ci => ci.ProductId).ToList();
            var catalogProducts = (await _productCatalogClient.GetProductsByIdsAsync(productIds))
                .ToDictionary(p => p.Id);

            var stockLevels = (await _inventoryClient.GetStockLevelsAsync(productIds)).ToDictionary(s => s.ProductId);

            var itemDtos = cartItems
                .Where(ci => catalogProducts.ContainsKey(ci.ProductId))
                .Select(ci =>
                {
                    var product = catalogProducts[ci.ProductId];
                    var availableQty = stockLevels.TryGetValue(ci.ProductId, out var stock) ? stock.QuantityAvailable : 0;

                    return new CartItemDto
                    {
                        ProductId = ci.ProductId,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = ci.Quantity,
                        ImageUrl = product.ImageUrl,
                        QuantityAvailable = availableQty
                    };
                })
                .ToList();

            return new CartDto
            {
                CartId = cart.Id,
                Items = itemDtos
            };
        }
    }
}