using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Constants;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Queries;

public sealed record ListOrdersQuery(string? Status, string? Search) : IRequest<IReadOnlyCollection<OrderListItemDto>>;

public sealed class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, IReadOnlyCollection<OrderListItemDto>>
{
    private readonly OrderDbContext _db;

    public ListOrdersQueryHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<OrderListItemDto>> Handle(
        ListOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<OrderStatus>(request.Status, true, out var status))
        {
            query = query.Where(order => order.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = Guid.TryParse(search, out var id)
                ? query.Where(order =>
                    order.Id == id ||
                    order.CustomerName.Contains(search) ||
                    order.CustomerEmail.Contains(search))
                : query.Where(order =>
                    order.CustomerName.Contains(search) ||
                    order.CustomerEmail.Contains(search));
        }

        return await query
            .OrderByDescending(order => order.CreatedAt)
            .Take(OrderLimits.OrderListPageSize)
            .Select(order => new OrderListItemDto(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.Status.ToString(),
                order.PaymentMethod.ToString(),
                order.CreatedAt,
                order.Items.Count))
            .ToListAsync(cancellationToken);
    }
}
