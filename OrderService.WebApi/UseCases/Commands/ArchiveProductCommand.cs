using MediatR;
using OrderService.DataAccess;

namespace OrderService.WebApi.UseCases.Commands;

public sealed record ArchiveProductCommand(Guid Id) : IRequest<bool>;

public sealed class ArchiveProductCommandHandler : IRequestHandler<ArchiveProductCommand, bool>
{
    private readonly OrderDbContext _db;

    public ArchiveProductCommandHandler(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FindAsync([request.Id], cancellationToken);
        if (product is null)
            return false;

        product.Archive();
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
