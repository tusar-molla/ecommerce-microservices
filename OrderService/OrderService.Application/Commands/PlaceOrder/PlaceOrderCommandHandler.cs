using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.PlaceOrder
{
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductCatalogClient _productCatalogClient;

        public PlaceOrderCommandHandler(
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IProductCatalogClient productCatalogClient)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _productCatalogClient = productCatalogClient;
        }

        public async Task<Guid> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
            if (cart is null)
            {
                throw new InvalidOperationException("Cannot place an order with an empty cart.");
            }

            var cartItems = (await _cartRepository.GetItemsAsync(cart.Id)).ToList();
            if (cartItems.Count == 0)
            {
                throw new InvalidOperationException("Cannot place an order with an empty cart.");
            }

            var productIds = cartItems.Select(ci => ci.ProductId).ToList();
            var catalogProducts = (await _productCatalogClient.GetProductsByIdsAsync(productIds))
                .ToDictionary(p => p.Id);

            var missingProducts = cartItems.Where(ci => !catalogProducts.ContainsKey(ci.ProductId)).ToList();
            if (missingProducts.Count > 0)
            {
                throw new InvalidOperationException(
                    "One or more items in your cart are no longer available. Please review your cart before checking out.");
            }

            var orderId = Guid.NewGuid();

            var orderItems = cartItems.Select(ci =>
            {
                var product = catalogProducts[ci.ProductId];
                return new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = ci.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = ci.Quantity
                };
            }).ToList();

            var order = new Order
            {
                Id = orderId,
                UserId = request.UserId,
                Status = OrderStatus.Pending,
                TotalAmount = orderItems.Sum(oi => oi.LineTotal),
                ShippingAddress = request.ShippingAddress
            };

            await _orderRepository.CreateAsync(order, orderItems);
            await _cartRepository.ClearCartAsync(cart.Id);

            return orderId;
        }
    }
}
