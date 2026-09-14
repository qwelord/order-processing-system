using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponseDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
{
    private readonly OrderDbContext _db;

    public GetOrderByIdQueryHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);

        if (order is null) return null;

        return new OrderResponseDto(
            order.Id,
            order.CustomerName,
            order.CustomerEmail,
            order.TotalAmount,
            order.Status.ToString(),
            order.PaymentMethod,
            order.CreatedAt,
            order.Items.Select(x => new OrderItemResponseDto(x.ProductId, x.ProductName, x.UnitPrice, x.Quantity, x.LineTotal)).ToList());
    }
}
