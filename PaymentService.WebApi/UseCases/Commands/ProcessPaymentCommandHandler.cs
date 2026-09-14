using FluentValidation;
using FluentValidation.Results;
using System.Text.Json;
using MediatR;
using PaymentService.DataAccess;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;
using PaymentService.WebApi.Services;

namespace PaymentService.WebApi.UseCases.Commands;

public record ProcessPaymentCommand(ProcessPaymentDto PaymentDto) : IRequest<PaymentResponseDto>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponseDto>
{
    private readonly PaymentDbContext _db;

    public ProcessPaymentCommandHandler(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task<PaymentResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.PaymentDto.Amount <= 0)
            throw new ValidationException(new[] { new ValidationFailure("Amount", "Payment amount must be positive.") });

        if (request.PaymentDto.PaymentMethod is not ("Card" or "CashOnDelivery"))
            throw new ValidationException(new[] { new ValidationFailure("PaymentMethod", "Unsupported payment method.") });

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.PaymentDto.OrderId,
            Amount = request.PaymentDto.Amount,
            PaymentMethod = request.PaymentDto.PaymentMethod,
            CardLast4 = request.PaymentDto.PaymentMethod == "Card" ? request.PaymentDto.CardLast4 : null,
            ProcessedAt = DateTime.UtcNow
        };

        if (payment.PaymentMethod == "Card")
        {
            if (string.IsNullOrWhiteSpace(payment.CardLast4) || payment.CardLast4.Length != 4 || !payment.CardLast4.All(char.IsDigit))
                throw new ValidationException(new[] { new ValidationFailure("CardLast4", "Card payment requires four digits from the card number.") });

            payment.Status = payment.CardLast4 == "0000" ? PaymentStatus.Failed : PaymentStatus.Completed;
        }
        else
        {
            payment.Status = PaymentStatus.Pending;
        }

        _db.Payments.Add(payment);

        if (payment.Status == PaymentStatus.Completed)
        {
            var eventMessage = new PaymentCompletedEvent(payment.OrderId, payment.Id, payment.Amount, "Completed", payment.ProcessedAt);
            _db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(PaymentCompletedEvent),
                Content = JsonSerializer.Serialize(eventMessage),
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new PaymentResponseDto(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Status.ToString(),
            payment.PaymentMethod,
            payment.CardLast4,
            payment.ProcessedAt);
    }
}
