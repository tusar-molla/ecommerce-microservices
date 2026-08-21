using MediatR;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.RemoveFromCart
{
    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, Unit>
    {
        private readonly ICartRepository _cartRepository;

        public RemoveFromCartCommandHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<Unit> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
            if (cart is null)
            {
                throw new InvalidOperationException("Cart not found.");
            }

            await _cartRepository.RemoveItemAsync(cart.Id, request.ProductId);

            return Unit.Value;
        }
    }
}
