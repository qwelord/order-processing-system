using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly OrderDbContext _db;

    public ProductsController(OrderDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Products.AsNoTracking();

        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x => x.Name.Contains(value) || x.Description.Contains(value));
        }

        var products = await query
            .OrderBy(x => x.Name)
            .Select(x => new ProductDto(x.Id, x.Name, x.Description, x.Price, x.StockQuantity, x.IsActive, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return NotFound();
        return Ok(new ProductDto(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.IsActive, product.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto, CancellationToken cancellationToken)
    {
        if (dto.Price <= 0 || dto.StockQuantity < 0) return BadRequest("Price must be positive and stock cannot be negative.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id },
            new ProductDto(product.Id, product.Name, product.Description, product.Price, product.StockQuantity, product.IsActive, product.CreatedAt));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductDto dto, CancellationToken cancellationToken)
    {
        if (dto.Price <= 0 || dto.StockQuantity < 0) return BadRequest("Price must be positive and stock cannot be negative.");

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return NotFound();

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null) return NotFound();
        product.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
