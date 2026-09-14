using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public record CreateOrderCommand(CreateOrderDto OrderDto) : IRequest<OrderResponseDto>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _db;

    public CreateOrderCommandHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var items = request.OrderDto.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new CreateOrderItemDto(group.Key, group.Sum(item => item.Quantity)))
            .ToList();

        var productIds = items.Select(item => item.ProductId).ToArray();
        var products = await _db.Products
            .Where(product => productIds.Contains(product.Id) && product.IsActive)
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        if (products.Count != productIds.Length)
            throw new ValidationException(new[]
            {
                new ValidationFailure("Items", "One or more products are unavailable.")
            });

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.OrderDto.CustomerName.Trim(),
            CustomerEmail = request.OrderDto.CustomerEmail.Trim().ToLowerInvariant(),
            PaymentMethod = request.OrderDto.PaymentMethod,
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            var product = products[item.ProductId];
            if (product.StockQuantity < item.Quantity)
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Items", $"Not enough stock for product '{product.Name}'.")
                });

            product.StockQuantity -= item.Quantity;

            var lineTotal = product.Price * item.Quantity;
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                LineTotal = lineTotal
            });

            order.TotalAmount += lineTotal;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(order);
    }

    private static OrderResponseDto ToResponse(Order order) => new(
        order.Id,
        order.CustomerName,
        order.CustomerEmail,
        order.TotalAmount,
        order.Status.ToString(),
        order.PaymentMethod,
        order.CreatedAt,
        order.Items.Select(item => new OrderItemResponseDto(
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity,
            item.LineTotal)).ToList());
}
