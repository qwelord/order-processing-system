using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Queries;

public record ListOrdersQuery(string? Status, string? Search) : IRequest<IReadOnlyCollection<OrderListItemDto>>;

public class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, IReadOnlyCollection<OrderListItemDto>>
{
    private readonly OrderDbContext _db;

    public ListOrdersQueryHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<OrderListItemDto>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var status))
            query = query.Where(x => x.Status == status);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            if (Guid.TryParse(search, out var id))
                query = query.Where(x => x.Id == id || x.CustomerName.Contains(search) || x.CustomerEmail.Contains(search));
            else
                query = query.Where(x => x.CustomerName.Contains(search) || x.CustomerEmail.Contains(search));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .Select(x => new OrderListItemDto(
                x.Id,
                x.CustomerName,
                x.TotalAmount,
                x.Status.ToString(),
                x.PaymentMethod,
                x.CreatedAt,
                x.Items.Count))
            .ToListAsync(cancellationToken);
    }
}
