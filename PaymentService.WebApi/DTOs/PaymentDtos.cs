namespace PaymentService.WebApi.DTOs;

public record ProcessPaymentDto(Guid OrderId, decimal Amount, string PaymentMethod, string? CardLast4);
public record PaymentResponseDto(Guid Id, Guid OrderId, decimal Amount, string Status, string PaymentMethod, string? CardLast4, DateTime ProcessedAt);
