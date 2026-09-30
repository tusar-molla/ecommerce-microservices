using ECommerce.Contracts.Events;
using MassTransit;
using MediatR;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentService.Application.Commands.ProcessIpnCallback
{
    public class ProcessIpnCallbackCommandHandler : IRequestHandler<ProcessIpnCallbackCommand, Unit>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IIpnCallbackRepository _ipnCallbackRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPublishEndpoint _publishEndpoint;

        public ProcessIpnCallbackCommandHandler(
            IPaymentRepository paymentRepository,
            IIpnCallbackRepository ipnCallbackRepository,
            IPaymentGateway paymentGateway,
            IPublishEndpoint publishEndpoint)
        {
            _paymentRepository = paymentRepository;
            _ipnCallbackRepository = ipnCallbackRepository;
            _paymentGateway = paymentGateway;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> Handle(ProcessIpnCallbackCommand request, CancellationToken cancellationToken)
        {
            var alreadyProcessed = await _ipnCallbackRepository.HasBeenProcessedAsync(request.TransactionId);
            if (alreadyProcessed)
            {
                return Unit.Value;
            }

            var payment = await _paymentRepository.GetByTransactionIdAsync(request.TransactionId);
            if (payment is null)
            {
                return Unit.Value;
            }

            var validation = await _paymentGateway.ValidateTransactionAsync(request.ValidationId);

            await _ipnCallbackRepository.MarkAsProcessedAsync(request.TransactionId);

            if (!validation.IsValid || validation.Amount != payment.Amount)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = !validation.IsValid
                    ? $"Gateway validation failed. Status: {validation.Status}"
                    : $"Amount mismatch. Expected {payment.Amount}, got {validation.Amount}";
                payment.GatewayValidationId = request.ValidationId;

                await _paymentRepository.UpdateAsync(payment);

                await _publishEndpoint.Publish(new PaymentFailedEvent
                {
                    OrderId = payment.OrderId,
                    Reason = payment.FailureReason
                });

                return Unit.Value;
            }

            payment.Status = PaymentStatus.Completed;
            payment.GatewayValidationId = request.ValidationId;

            await _paymentRepository.UpdateAsync(payment);

            await _publishEndpoint.Publish(new PaymentCompletedEvent
            {
                OrderId = payment.OrderId,
                PaymentId = payment.Id,
                AmountCharged = payment.Amount
            });

            return Unit.Value;
        }
    }
}
