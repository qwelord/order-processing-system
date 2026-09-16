using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.UseCases.Commands;
using OrderService.WebApi.UseCases.Queries;

namespace OrderService.WebApi.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<OrderListItemDto>>> GetOrders(
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListOrdersQuery(status, search), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new CreateOrderCommand(dto), cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<ActionResult<OrderResponseDto>> PayOrder(
        Guid id,
        [FromBody] PayOrderDto dto,
        CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new PayOrderCommand(id, dto), cancellationToken);
        return Ok(order);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponseDto>> GetOrderById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderResponseDto>> CancelOrder(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new CancelOrderCommand(id), cancellationToken);
        return Ok(order);
    }
}
