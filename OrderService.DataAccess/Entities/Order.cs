
namespace OrderService.DataAccess.Entities;

public enum OrderStatus
{
    PendingPayment,
    Paid,
    Cancelled
}

public sealed class Order
{
    private Order()
    {
    }

    private Order(
        Guid id,
        string customerName,
        string customerEmail,
        PaymentMethod paymentMethod,
        DateTime createdAt)
    {
        Id = id;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        PaymentMethod = paymentMethod;
        CreatedAt = createdAt;
        Status = OrderStatus.PendingPayment;
    }

    public Guid Id { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];

    public static Order Create(
        string customerName,
        string customerEmail,
        PaymentMethod paymentMethod,
        DateTime createdAt)
    {
        return new Order(
            Guid.NewGuid(),
            customerName,
            customerEmail,
            paymentMethod,
            createdAt);
    }

    public void AddItem(OrderItem item)
    {
        Items.Add(item);
        TotalAmount += item.LineTotal;
    }

    public void MarkPaid()
    {
        Status = OrderStatus.Paid;
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
    }

    public bool UsesCardPayment() => PaymentMethod == PaymentMethod.Card;
}
