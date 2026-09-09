namespace OrderService.DataAccess.Entities;

public enum OrderStatus
{
    Created = 0,
    PendingPayment = 1,
    Paid = 2,
    Failed = 3
}

public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}