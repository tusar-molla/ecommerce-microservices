using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.AddToCart
{
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Guid>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IInventoryClient _inventoryClient;

        public AddToCartCommandHandler(ICartRepository cartRepository, IInventoryClient inventoryClient)
        {
            _cartRepository = cartRepository;
            _inventoryClient = inventoryClient;
        }
        public async Task<Guid> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {

            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);

            var existingQuantityInCart = 0;
            if (cart is not null)
            {
                var existingItem = await _cartRepository.GetItemAsync(cart.Id, request.ProductId);
                existingQuantityInCart = existingItem?.Quantity ?? 0;
            }

            var totalRequestedQuantity = existingQuantityInCart + request.Quantity;

            var stockLevels = await _inventoryClient.GetStockLevelsAsync(new[] { request.ProductId });
            var stock = stockLevels.FirstOrDefault();
            var available = stock?.QuantityAvailable ?? 0;

            if (available < totalRequestedQuantity)
            {
                throw new InvalidOperationException(
                    available == 0
                        ? "This item is currently out of stock."
                        : existingQuantityInCart > 0
                            ? $"You already have {existingQuantityInCart} in your cart. Only {available} available in total — please reduce the quantity."
                            : $"Only {available} unit(s) available. Please reduce the quantity.");
            }

            Guid cartId;
            if (cart is null)
            {
                cartId = await _cartRepository.CreateCartAsync(request.UserId);
            }
            else
            {
                cartId = cart.Id;
            }

            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cartId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            await _cartRepository.AddOrUpdateItemAsync(cartItem);

            return cartId;
        }
    }
}
