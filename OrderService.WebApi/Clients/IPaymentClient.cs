using Refit;

namespace OrderService.WebApi.Clients;

public record ProcessPaymentRequest(Guid OrderId, decimal Amount);
public record ProcessPaymentResponse(Guid Id, Guid OrderId, decimal Amount, string Status, DateTime ProcessedAt);

public interface IPaymentClient
{
    [Post("/api/payments/process")]
    Task<ProcessPaymentResponse> ProcessPaymentAsync([Body] ProcessPaymentRequest request);
}