namespace OrderService.DataAccess.Entities;

public enum OrderStatus
{
    PendingPayment = 0,
    Paid = 1,
    Cancelled = 2
}

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}