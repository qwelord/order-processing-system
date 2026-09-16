using OrderService.DataAccess.Entities;

namespace OrderService.WebApi.DTOs;

public sealed record CreateOrderItemDto(Guid ProductId, int Quantity);

public sealed record CreateOrderDto(
    string CustomerName,
    string CustomerEmail,
    IReadOnlyCollection<CreateOrderItemDto> Items,
    string PaymentMethod);

public sealed record PayOrderDto(string CardLast4);

public sealed record OrderItemResponseDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record OrderResponseDto(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    DateTime CreatedAt,
    IReadOnlyCollection<OrderItemResponseDto> Items)
{
    public static OrderResponseDto FromEntity(Order order) => new(
        order.Id,
        order.CustomerName,
        order.CustomerEmail,
        order.TotalAmount,
        order.Status.ToString(),
        order.PaymentMethod.ToString(),
        order.CreatedAt,
        order.Items.Select(item => new OrderItemResponseDto(
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity,
            item.LineTotal)).ToList());
}

public sealed record OrderListItemDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    DateTime CreatedAt,
    int ItemCount);

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt)
{
    public static ProductDto FromEntity(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.IsActive,
        product.CreatedAt);
}

public sealed record CreateProductDto(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity);

public sealed record UpdateProductDto(
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive);
