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
    string? CardLast4,
    DateTime ProcessedAt);

public interface IPaymentClient
{
    [Post("/api/payments/process")]
    Task<ProcessPaymentResponse> ProcessPaymentAsync([Body] ProcessPaymentRequest request);

    [Get("/api/payments")]
    Task<IReadOnlyList<ProcessPaymentResponse>> GetPaymentsAsync([Query] Guid orderId);
}
