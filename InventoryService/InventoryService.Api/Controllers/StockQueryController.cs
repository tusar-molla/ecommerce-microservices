using InventoryService.Application.Queries.GetStockLevels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers
{
    [Route("api/stock-levels")]
    [ApiController]
    public class StockQueryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StockQueryController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> GetStockLevels([FromQuery] List<Guid> productIds)
        {
            var levels = await _mediator.Send(new GetStockLevelsQuery { ProductIds = productIds });
            return Ok(levels);
        }
    }
}
