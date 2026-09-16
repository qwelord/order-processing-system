using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest<OrderResponseDto>;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _db;

    public CancelOrderCommandHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<OrderResponseDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == request.OrderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order was not found.");

        if (order.Status == OrderStatus.Paid)
            throw new ValidationException([new ValidationFailure(nameof(CancelOrderCommand.OrderId), "Paid orders cannot be cancelled.")]);

        if (order.Status == OrderStatus.Cancelled)
            return OrderResponseDto.FromEntity(order);

        var productIds = order.Items.Select(item => item.ProductId).ToArray();
        var products = await _db.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
                product.ReleaseStock(item.Quantity);
        }

        order.Cancel();
        await _db.SaveChangesAsync(cancellationToken);

        return OrderResponseDto.FromEntity(order);
    }
}
