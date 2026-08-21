using MediatR;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.ClearCart
{
    public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Unit>
    {
        private readonly ICartRepository _cartRepository;

        public ClearCartCommandHandler(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }
        public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
            if (cart is null)
            {
                return Unit.Value; // nothing to clear, not an error
            }

            await _cartRepository.ClearCartAsync(cart.Id);

            return Unit.Value;
        }
    }
}
