using PaymentService.DataAccess.Entities;

namespace PaymentService.WebApi.DTOs;

public sealed record ProcessPaymentDto(
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    string? CardLast4);

public sealed record PaymentResponseDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Status,
    string PaymentMethod,
    string? CardLast4,
    DateTime ProcessedAt)
{
    public static PaymentResponseDto FromEntity(Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.Amount,
        payment.Status.ToString(),
        payment.PaymentMethod.ToString(),
        payment.CardLast4,
        payment.ProcessedAt);
}
