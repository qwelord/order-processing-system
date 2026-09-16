using FluentValidation;
using FluentValidation.Results;
using MediatR;
using OrderService.DataAccess.Constants;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public sealed record CreateOrderCommand(CreateOrderDto OrderDto) : IRequest<OrderResponseDto>;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
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
            throw new ValidationException([new ValidationFailure("Items", "One or more products are unavailable.")]);

        var order = Order.Create(
            request.OrderDto.CustomerName.Trim(),
            request.OrderDto.CustomerEmail.Trim().ToLowerInvariant(),
            PaymentMethods.Parse(request.OrderDto.PaymentMethod),
            DateTime.UtcNow);

        foreach (var item in items)
        {
            var product = products[item.ProductId];
            if (!product.TryReserveStock(item.Quantity))
                throw new ValidationException([new ValidationFailure("Items", $"Not enough stock for product '{product.Name}'.")]);

            order.AddItem(OrderItem.Create(
                product.Id,
                product.Name,
                product.Price,
                item.Quantity));
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return OrderResponseDto.FromEntity(order);
    }
}
