using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Queries;

public sealed record ListProductsQuery(string? Search, bool IncludeInactive) : IRequest<IReadOnlyCollection<ProductDto>>;

public sealed class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, IReadOnlyCollection<ProductDto>>
{
    private readonly OrderDbContext _db;

    public ListProductsQueryHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Products.AsNoTracking();

        if (!request.IncludeInactive)
            query = query.Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(product =>
                product.Name.Contains(search) || product.Description.Contains(search));
        }

        return await query
            .OrderBy(product => product.Name)
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.IsActive,
                product.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
