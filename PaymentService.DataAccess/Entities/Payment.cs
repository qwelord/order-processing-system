namespace PaymentService.DataAccess.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2
}

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime ProcessedAt { get; set; }
}