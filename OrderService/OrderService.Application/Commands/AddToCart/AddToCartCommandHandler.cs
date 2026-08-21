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

        public AddToCartCommandHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }
        public async Task<Guid> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);

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
