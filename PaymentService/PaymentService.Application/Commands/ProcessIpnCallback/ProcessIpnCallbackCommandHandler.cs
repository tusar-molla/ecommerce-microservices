using ECommerce.Contracts.Events;
using MediatR;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;

namespace PaymentService.Application.Commands.ProcessIpnCallback;

public class ProcessIpnCallbackCommandHandler : IRequestHandler<ProcessIpnCallbackCommand, Unit>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IIpnCallbackRepository _ipnCallbackRepository;
    private readonly IPaymentGateway _paymentGateway;

    public ProcessIpnCallbackCommandHandler(
        IPaymentRepository paymentRepository,
        IIpnCallbackRepository ipnCallbackRepository,
        IPaymentGateway paymentGateway)
    {
        _paymentRepository = paymentRepository;
        _ipnCallbackRepository = ipnCallbackRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<Unit> Handle(ProcessIpnCallbackCommand request, CancellationToken cancellationToken)
    {
        if (await _ipnCallbackRepository.HasBeenProcessedAsync(request.TransactionId))
        {
            return Unit.Value;
        }

        var payment = await _paymentRepository.GetByTransactionIdAsync(request.TransactionId);
        if (payment is null)
        {
            return Unit.Value;
        }

        var validation = await _paymentGateway.ValidateTransactionAsync(request.ValidationId);

        payment.GatewayValidationId = request.ValidationId;
        OutboxMessage outbox;

        if (validation.IsValid && validation.Amount == payment.Amount)
        {
            payment.Status = PaymentStatus.Completed;
            payment.FailureReason = null;

            outbox = OutboxMessage.From(new PaymentCompletedEvent
            {
                OrderId = payment.OrderId,
                PaymentId = payment.Id,
                AmountCharged = payment.Amount
            });
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = !validation.IsValid
                ? $"Gateway validation failed. Status: {validation.Status}"
                : $"Amount mismatch. Expected {payment.Amount}, got {validation.Amount}";

            outbox = OutboxMessage.From(new PaymentFailedEvent
            {
                OrderId = payment.OrderId,
                Reason = payment.FailureReason
            });
        }

        await _paymentRepository.CompleteFromIpnAsync(payment, request.TransactionId, outbox);

        return Unit.Value;
    }
}