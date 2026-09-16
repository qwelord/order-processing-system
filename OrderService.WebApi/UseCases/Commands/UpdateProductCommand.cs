using MediatR;
using OrderService.DataAccess;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public sealed record UpdateProductCommand(Guid Id, UpdateProductDto ProductDto) : IRequest<ProductDto?>;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly OrderDbContext _db;

    public UpdateProductCommandHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FindAsync([request.Id], cancellationToken);
        if (product is null)
            return null;

        var dto = request.ProductDto;
        product.Update(
            dto.Name.Trim(),
            dto.Description?.Trim() ?? string.Empty,
            dto.Price,
            dto.StockQuantity,
            dto.IsActive);

        await _db.SaveChangesAsync(cancellationToken);
        return ProductDto.FromEntity(product);
    }
}
