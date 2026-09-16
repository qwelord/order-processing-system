using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Queries;

public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponseDto?>;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
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
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == request.OrderId, cancellationToken);

        return order is null ? null : OrderResponseDto.FromEntity(order);
    }
}
