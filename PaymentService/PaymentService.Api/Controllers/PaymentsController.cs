using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands.ProcessIpnCallback;
using PaymentService.Application.Queries.GetMyPayments;
using PaymentService.Application.Queries.GetPaymentByOrderId;
using PaymentService.Application.Queries.GetPaymentRedirectUrl;
using System.Security.Claims;

namespace PaymentService.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        [Authorize]
        [HttpGet("{orderId}/redirect-url")]
        public async Task<IActionResult> GetRedirectUrl(Guid orderId)
        {
            var url = await _mediator.Send(new GetPaymentRedirectUrlQuery { OrderId = orderId, UserId = GetCurrentUserId() });

            if (url is null)
            {
                return NotFound(new { Message = "Payment not found, not yet initiated, or does not belong to you." });
            }

            return Ok(new { GatewayPageUrl = url });
        }

        [Authorize]
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            var payment = await _mediator.Send(new GetPaymentByOrderIdQuery { OrderId = orderId, UserId = GetCurrentUserId() });

            if (payment is null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [Authorize]
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var payments = await _mediator.Send(new GetMyPaymentsQuery { UserId = GetCurrentUserId() });
            return Ok(payments);
        }

        [AllowAnonymous]
        [HttpPost("success")]
        public IActionResult Success([FromForm] IFormCollection form)
        {
            return Ok(new { Message = "Payment completed. You may close this window.", Note = "Final confirmation happens server-side via IPN." });
        }

        [AllowAnonymous]
        [HttpPost("fail")]
        public IActionResult Fail([FromForm] IFormCollection form)
        {
            return Ok(new { Message = "Payment failed or was declined." });
        }

        [AllowAnonymous]
        [HttpPost("cancel")]
        public IActionResult Cancel([FromForm] IFormCollection form)
        {
            return Ok(new { Message = "Payment was cancelled." });
        }

        [AllowAnonymous]
        [HttpPost("ipn")]
        public async Task<IActionResult> Ipn([FromForm] IFormCollection form)
        {
            var command = new ProcessIpnCallbackCommand
            {
                TransactionId = form["tran_id"].ToString(),
                ValidationId = form["val_id"].ToString(),
                Status = form["status"].ToString()
            };

            await _mediator.Send(command);

            return Ok();
        }
    }
}
