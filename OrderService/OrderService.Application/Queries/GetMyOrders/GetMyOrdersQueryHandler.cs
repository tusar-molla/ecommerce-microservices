using MediatR;
using OrderService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Queries.GetMyOrders
{
    public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, IEnumerable<OrderSummaryDto>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetMyOrdersQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.GetByUserIdAsync(request.UserId);

            return orders.Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            });
        }
    }
}
