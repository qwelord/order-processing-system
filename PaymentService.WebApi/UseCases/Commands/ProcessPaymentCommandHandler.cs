using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;
using PaymentService.WebApi.Services;
using System.Text.Json;

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
        var dto = request.PaymentDto;

        if (dto.Amount <= 0)
            throw new ValidationException(new[]
            {
                new ValidationFailure("Amount", "Payment amount must be positive.")
            });

        if (dto.PaymentMethod is not ("Card" or "CashOnDelivery"))
            throw new ValidationException(new[]
            {
                new ValidationFailure("PaymentMethod", "Unsupported payment method.")
            });

        var existingPayment = await _db.Payments
            .AsNoTracking()
            .SingleOrDefaultAsync(payment => payment.OrderId == dto.OrderId, cancellationToken);

        if (existingPayment is not null)
        {
            if (existingPayment.Amount != dto.Amount || existingPayment.PaymentMethod != dto.PaymentMethod)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("OrderId", "A different payment already exists for this order.")
                });
            }

            return ToResponse(existingPayment);
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            CardLast4 = dto.PaymentMethod == "Card" ? dto.CardLast4 : null,
            ProcessedAt = DateTime.UtcNow
        };

        payment.Status = dto.PaymentMethod == "Card"
            ? dto.CardLast4 == "0000" ? PaymentStatus.Failed : PaymentStatus.Completed
            : PaymentStatus.Pending;

        _db.Payments.Add(payment);
        AddOutboxMessage(payment);
        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(payment);
    }

    private void AddOutboxMessage(Payment payment)
    {
        var message = new PaymentProcessedEvent(
            payment.OrderId,
            payment.Id,
            payment.Amount,
            payment.Status.ToString(),
            payment.ProcessedAt);

        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(PaymentProcessedEvent),
            Content = JsonSerializer.Serialize(message),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static PaymentResponseDto ToResponse(Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.Amount,
        payment.Status.ToString(),
        payment.PaymentMethod,
        payment.CardLast4,
        payment.ProcessedAt);
}
