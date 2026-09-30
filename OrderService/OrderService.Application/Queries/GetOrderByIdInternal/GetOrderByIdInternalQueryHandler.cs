using MediatR;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Queries.GetOrderByIdInternal
{
    public class GetOrderByIdInternalQueryHandler : IRequestHandler<GetOrderByIdInternalQuery, OrderService.Application.Queries.GetOrderById.OrderDto?>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdInternalQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderService.Application.Queries.GetOrderById.OrderDto?> Handle(GetOrderByIdInternalQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order is null) return null;

            var items = await _orderRepository.GetItemsAsync(order.Id);

            return new OrderService.Application.Queries.GetOrderById.OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                CreatedAt = order.CreatedAt,
                Items = items.Select(i => new OrderService.Application.Queries.GetOrderById.OrderItemDto
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
