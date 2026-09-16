using PaymentService.DataAccess.Constants;


namespace PaymentService.DataAccess.Entities;

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed
}

public sealed class Payment
{
    private Payment()
    {
    }

    private Payment(
        Guid id,
        Guid orderId,
        decimal amount,
        PaymentStatus status,
        PaymentMethod paymentMethod,
        string? cardLast4,
        DateTime processedAt)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Status = status;
        PaymentMethod = paymentMethod;
        CardLast4 = cardLast4;
        ProcessedAt = processedAt;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string? CardLast4 { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    public static Payment Create(
        Guid orderId,
        decimal amount,
        PaymentMethod paymentMethod,
        string? cardLast4,
        DateTime processedAt)
    {
        var status = paymentMethod == PaymentMethod.Card
            ? cardLast4 == DemoPaymentData.DeclinedCardLast4
                ? PaymentStatus.Failed
                : PaymentStatus.Completed
            : PaymentStatus.Pending;

        return new Payment(
            Guid.NewGuid(),
            orderId,
            amount,
            status,
            paymentMethod,
            paymentMethod == PaymentMethod.Card ? cardLast4 : null,
            processedAt);
    }

    public bool Matches(decimal amount, PaymentMethod paymentMethod, string? cardLast4) =>
        Amount == amount &&
        PaymentMethod == paymentMethod &&
        CardLast4 == (paymentMethod == PaymentMethod.Card ? cardLast4 : null);
}
