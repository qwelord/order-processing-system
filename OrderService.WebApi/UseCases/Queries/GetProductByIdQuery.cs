using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Queries;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly OrderDbContext _db;

    public GetProductByIdQueryHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(product => product.Id == request.Id)
            .Select(product => new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.IsActive,
                product.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
