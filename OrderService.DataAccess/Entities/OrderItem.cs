namespace OrderService.DataAccess.Entities;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    private OrderItem(
        Guid id,
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity,
        decimal lineTotal)
    {
        Id = id;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        LineTotal = lineTotal;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal { get; private set; }
    public Order Order { get; private set; } = null!;

    public static OrderItem Create(
        Guid productId,
        string productName,
        decimal unitPrice,
        int quantity)
    {
        return new OrderItem(
            Guid.NewGuid(),
            productId,
            productName,
            unitPrice,
            quantity,
            unitPrice * quantity);
    }
}
