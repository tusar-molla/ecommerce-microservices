using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands.AddToCart;
using OrderService.Application.Commands.ClearCart;
using OrderService.Application.Commands.RemoveFromCart;
using OrderService.Application.Queries.GetCart;
using System.Security.Claims;

namespace OrderService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var result = await _mediator.Send(new GetCartQuery { UserId = GetCurrentUserId() });
            return Ok(result);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var command = new AddToCartCommand
            {
                UserId = GetCurrentUserId(),
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            var cartId = await _mediator.Send(command);
            return Ok(new { CartId = cartId });
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(Guid productId)
        {
            await _mediator.Send(new RemoveFromCartCommand { UserId = GetCurrentUserId(), ProductId = productId });
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await _mediator.Send(new ClearCartCommand { UserId = GetCurrentUserId() });
            return NoContent();
        }
    }

    public class AddToCartRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

