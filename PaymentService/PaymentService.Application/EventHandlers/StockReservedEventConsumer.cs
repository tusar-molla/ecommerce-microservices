using ECommerce.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Configuration;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.EventHandlers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly IIdentityServiceClient _identityServiceClient;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IConfiguration _configuration;

        public StockReservedEventConsumer(
            IOrderServiceClient orderServiceClient,
            IIdentityServiceClient identityServiceClient,
            IPaymentRepository paymentRepository,
            IPaymentGateway paymentGateway,
            IConfiguration configuration)
        {
            _orderServiceClient = orderServiceClient;
            _identityServiceClient = identityServiceClient;
            _paymentRepository = paymentRepository;
            _paymentGateway = paymentGateway;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var orderId = context.Message.OrderId;

            var order = await _orderServiceClient.GetOrderAsync(orderId);
            if (order is null)
            {
                return;
            }

            var existingPayment = await _paymentRepository.GetByOrderIdAsync(orderId);
            if (existingPayment is not null)
            {
                return;
            }

            var user = await _identityServiceClient.GetUserAsync(order.UserId);

            var transactionId = $"TXN_{Guid.NewGuid():N}"[..30];
            var baseUrl = _configuration["App:BaseUrl"]!;

            var initiateResult = await _paymentGateway.InitiateSessionAsync(new InitiateSessionRequest
            {
                TransactionId = transactionId,
                Amount = order.TotalAmount,
                Currency = "BDT",
                CustomerName = user?.FullName ?? "Customer",
                CustomerEmail = user?.Email ??"customer@example.com",
                SuccessUrl = $"{baseUrl}/api/payments/success",
                FailUrl = $"{baseUrl}/api/payments/fail",
                CancelUrl = $"{baseUrl}/api/payments/cancel",
                IpnUrl = $"{baseUrl}/api/payments/ipn"
            });

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                UserId = order.UserId,
                Amount = order.TotalAmount,
                Currency = "BDT",
                TransactionId = transactionId,
                Status = initiateResult.Success ? PaymentStatus.AwaitingGatewayRedirect : PaymentStatus.Failed,
                GatewaySessionKey = initiateResult.SessionKey,
                FailureReason = initiateResult.FailureReason,
                GatewayPageUrl = initiateResult.GatewayPageUrl
            };

            await _paymentRepository.CreateAsync(payment);

            if (!initiateResult.Success)
            {
                await context.Publish(new PaymentFailedEvent
                {
                    OrderId = orderId,
                    Reason = initiateResult.FailureReason ?? "Payment session could not be created"
                });
            }
        }
    }
}
