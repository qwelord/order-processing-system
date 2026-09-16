using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess;
using PaymentService.DataAccess.Constants;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;
using PaymentService.WebApi.Services;
using System.Text.Json;

namespace PaymentService.WebApi.UseCases.Commands;

public sealed record ProcessPaymentCommand(ProcessPaymentDto PaymentDto) : IRequest<PaymentResponseDto>;

public sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponseDto>
{
    private readonly PaymentDbContext _db;

    public ProcessPaymentCommandHandler(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task<PaymentResponseDto> Handle(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.PaymentDto;
        var paymentMethod = PaymentMethods.Parse(dto.PaymentMethod);
        var existingPayment = await _db.Payments
            .AsNoTracking()
            .SingleOrDefaultAsync(payment => payment.OrderId == dto.OrderId, cancellationToken);

        if (existingPayment is not null)
        {
            if (!existingPayment.Matches(dto.Amount, paymentMethod, dto.CardLast4))
                throw new ValidationException([new ValidationFailure(nameof(ProcessPaymentCommand.PaymentDto), "A different payment already exists for this order.")]);

            return PaymentResponseDto.FromEntity(existingPayment);
        }

        var payment = Payment.Create(
            dto.OrderId,
            dto.Amount,
            paymentMethod,
            dto.CardLast4,
            DateTime.UtcNow);

        _db.Payments.Add(payment);
        AddOutboxMessage(payment);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var concurrentPayment = await _db.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(payment => payment.OrderId == dto.OrderId, cancellationToken);

            if (concurrentPayment is null)
                throw;

            if (!concurrentPayment.Matches(dto.Amount, paymentMethod, dto.CardLast4))
                throw new ValidationException([new ValidationFailure(nameof(ProcessPaymentCommand.PaymentDto), "A different payment already exists for this order.")]);

            return PaymentResponseDto.FromEntity(concurrentPayment);
        }

        return PaymentResponseDto.FromEntity(payment);
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
}
