using MediatR;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public sealed record CreateProductCommand(CreateProductDto ProductDto) : IRequest<ProductDto>;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly OrderDbContext _db;

    public CreateProductCommandHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.ProductDto;
        var product = Product.Create(
            dto.Name.Trim(),
            dto.Description?.Trim() ?? string.Empty,
            dto.Price,
            dto.StockQuantity,
            DateTime.UtcNow);

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return ProductDto.FromEntity(product);
    }
}
