using Refit;

namespace OrderService.WebApi.Clients;

public record ProcessPaymentRequest(
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    string? CardLast4);

public record ProcessPaymentResponse(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Status,
    string PaymentMethod,
    DateTime ProcessedAt);

public interface IPaymentClient
{
    [Post("/api/payments/process")]
    Task<ProcessPaymentResponse> ProcessPaymentAsync([Body] ProcessPaymentRequest request);
}
