using InventoryService.Application.Commands.AdjustStock;
using InventoryService.Application.Commands.SetInitialStock;
using InventoryService.Application.Queries.GetMissingStock;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StockController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        public async Task<IActionResult> SetInitialStock([FromBody] SetInitialStockCommand command)
        {
            var stockId = await _mediator.Send(command);
            return Ok(new { StockId = stockId });
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustStockCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("missing")]
        public async Task<IActionResult> GetMissingStock()
        {
            var missingProductIds = await _mediator.Send(new GetMissingStockQuery());
            return Ok(missingProductIds);
        }
    }
}
