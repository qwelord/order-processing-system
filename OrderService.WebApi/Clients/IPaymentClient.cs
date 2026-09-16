using Refit;

namespace OrderService.WebApi.Clients;

public sealed record ProcessPaymentRequest(
    Guid OrderId,
    decimal Amount,
    string PaymentMethod,
    string? CardLast4);

public sealed record ProcessPaymentResponse(
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
    Task<ProcessPaymentResponse> ProcessPaymentAsync(
        [Body] ProcessPaymentRequest request,
        CancellationToken cancellationToken = default);

    [Get("/api/payments")]
    Task<IReadOnlyList<ProcessPaymentResponse>> GetPaymentsAsync(
        [Query] Guid orderId,
        CancellationToken cancellationToken = default);
}
