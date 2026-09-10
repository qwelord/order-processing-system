namespace NotificationService.WebApi.Hubs;

public interface INotificationClient
{
    Task ReceivePaymentUpdate(PaymentNotificationContract notification);
}

public record PaymentNotificationContract(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string Status,
    DateTime Timestamp);