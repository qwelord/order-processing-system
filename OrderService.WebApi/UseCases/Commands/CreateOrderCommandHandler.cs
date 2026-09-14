using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.Clients;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public record CreateOrderCommand(CreateOrderDto OrderDto) : IRequest<OrderResponseDto>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _db;
    private readonly IPaymentClient _paymentClient;

    public CreateOrderCommandHandler(OrderDbContext db, IPaymentClient paymentClient)
    {
        _db = db;
        _paymentClient = paymentClient;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var normalizedItems = request.OrderDto.Items
            .GroupBy(x => x.ProductId)
            .Select(x => new CreateOrderItemDto(x.Key, x.Sum(i => i.Quantity)))
            .ToList();

        var productIds = normalizedItems.Select(x => x.ProductId).ToArray();
        var products = await _db.Products
            .Where(x => productIds.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (products.Count != productIds.Length)
            throw new ValidationException(new[] { new ValidationFailure("Items", "One or more products are unavailable.") });

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.OrderDto.CustomerName.Trim(),
            CustomerEmail = request.OrderDto.CustomerEmail.Trim().ToLowerInvariant(),
            PaymentMethod = request.OrderDto.PaymentMethod,
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in normalizedItems)
        {
            var product = products[item.ProductId];
            if (product.StockQuantity < item.Quantity)
                throw new ValidationException(new[] { new ValidationFailure("Items", $"Not enough stock for product '{product.Name}'.") });

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

        try
        {
            var payment = await _paymentClient.ProcessPaymentAsync(
                new ProcessPaymentRequest(order.Id, order.TotalAmount, request.OrderDto.PaymentMethod, request.OrderDto.CardLast4));

            if (payment.Status == "Completed")
                order.Status = OrderStatus.Paid;
        }
        catch
        {
            await RestoreStockAsync(order, cancellationToken);
            order.Status = OrderStatus.Cancelled;
            await _db.SaveChangesAsync(cancellationToken);
            throw;
        }

        if (order.Status != OrderStatus.Paid && request.OrderDto.PaymentMethod == "Card")
        {
            await RestoreStockAsync(order, cancellationToken);
            order.Status = OrderStatus.Cancelled;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(order);
    }

    private async Task RestoreStockAsync(Order order, CancellationToken cancellationToken)
    {
        var productIds = order.Items.Select(x => x.ProductId).ToArray();
        var products = await _db.Products.Where(x => productIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);

        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
                product.StockQuantity += item.Quantity;
        }
    }

    private static OrderResponseDto ToResponse(Order order) => new(
        order.Id,
        order.CustomerName,
        order.CustomerEmail,
        order.TotalAmount,
        order.Status.ToString(),
        order.PaymentMethod,
        order.CreatedAt,
        order.Items.Select(x => new OrderItemResponseDto(x.ProductId, x.ProductName, x.UnitPrice, x.Quantity, x.LineTotal)).ToList());
}
