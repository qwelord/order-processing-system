namespace OrderService.DataAccess.Entities;

public sealed class Product
{
    private Product()
    {
    }

    private Product(
        Guid id,
        string name,
        string description,
        decimal price,
        int stockQuantity,
        bool isActive,
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Product Create(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        DateTime createdAt)
    {
        return new Product(
            Guid.NewGuid(),
            name,
            description,
            price,
            stockQuantity,
            true,
            createdAt);
    }

    public void Update(string name, string description, decimal price, int stockQuantity, bool isActive)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = isActive;
    }

    public bool TryReserveStock(int quantity)
    {
        if (StockQuantity < quantity)
            return false;

        StockQuantity -= quantity;
        return true;
    }

    public void ReleaseStock(int quantity)
    {
        StockQuantity += quantity;
    }

    public void Archive()
    {
        IsActive = false;
    }
}
