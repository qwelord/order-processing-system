namespace PaymentService.WebApi.DTOs;

public record ProcessPaymentDto(Guid OrderId, decimal Amount);
public record PaymentResponseDto(Guid Id, Guid OrderId, decimal Amount, string Status, DateTime ProcessedAt);