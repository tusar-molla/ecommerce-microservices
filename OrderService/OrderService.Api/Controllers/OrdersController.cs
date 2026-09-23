using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands.PlaceOrder;
using OrderService.Application.Queries.GetOrderById;
using OrderService.Application.Queries.GetMyOrders;
using System.Security.Claims;

namespace OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            var command = new PlaceOrderCommand
            {
                UserId = GetCurrentUserId(),
                ShippingAddress = request.ShippingAddress
            };

            var orderId = await _mediator.Send(command);
            return Ok(new { OrderId = orderId });
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _mediator.Send(new GetOrderByIdQuery { OrderId = orderId, UserId = GetCurrentUserId() });

            if (order is null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var orders = await _mediator.Send(new GetMyOrdersQuery { UserId = GetCurrentUserId() });
            return Ok(orders);
        }
    }

    public class PlaceOrderRequest
    {
        public string ShippingAddress { get; set; } = string.Empty;
    }
}