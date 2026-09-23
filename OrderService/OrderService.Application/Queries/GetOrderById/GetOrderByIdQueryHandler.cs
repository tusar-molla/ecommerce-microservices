using MediatR;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);

            if (order is null || order.UserId != request.UserId)
            {
                return null;
            }

            var items = await _orderRepository.GetItemsAsync(order.Id);

            return new OrderDto
            {
                Id = order.Id,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CreatedAt = order.CreatedAt,
                Items = items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}
