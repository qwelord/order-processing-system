using System.ComponentModel.DataAnnotations;

namespace OrderService.WebApi.DTOs;

public record CreateOrderItemDto(Guid ProductId, int Quantity);

public record CreateOrderDto(
    string CustomerName,
    string CustomerEmail,
    IReadOnlyCollection<CreateOrderItemDto> Items,
    string PaymentMethod,
    string? CardLast4);

public record OrderItemResponseDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public record OrderResponseDto(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    DateTime CreatedAt,
    IReadOnlyCollection<OrderItemResponseDto> Items);

public record OrderListItemDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    DateTime CreatedAt,
    int ItemCount);

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt);

public record CreateProductDto(
    [property: Required] string Name,
    string? Description,
    decimal Price,
    int StockQuantity);

public record UpdateProductDto(
    [property: Required] string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    bool IsActive);
