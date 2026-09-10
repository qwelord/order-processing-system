namespace OrderService.WebApi.DTOs;

public record CreateOrderDto(string CustomerName, decimal TotalAmount);

public record OrderResponseDto(
    Guid Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt);