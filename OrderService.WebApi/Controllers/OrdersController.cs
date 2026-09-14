using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.UseCases.Commands;
using OrderService.WebApi.UseCases.Queries;

namespace OrderService.WebApi.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly OrderDbContext _db;

    public OrdersController(IMediator mediator, OrderDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? status, [FromQuery] string? search, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ListOrdersQuery(status, search), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new CreateOrderCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> PayOrder(Guid id, [FromBody] PayOrderDto dto, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new PayOrderCommand(id, dto), cancellationToken);
        return Ok(order);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null) return NotFound();
        if (order.Status == OrderStatus.Cancelled) return Ok();
        if (order.Status == OrderStatus.Paid) return Conflict("Paid orders cannot be cancelled from this demo API.");

        var productIds = order.Items.Select(x => x.ProductId).ToArray();
        var products = await _db.Products.Where(x => productIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var item in order.Items)
            if (products.TryGetValue(item.ProductId, out var product))
                product.StockQuantity += item.Quantity;

        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
