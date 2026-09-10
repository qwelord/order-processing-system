using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.WebApi.Clients;
using OrderService.WebApi.Commands;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.Queries;

namespace OrderService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentClient _paymentClient;

    public OrdersController(IMediator mediator, IPaymentClient paymentClient)
    {
        _mediator = mediator;
        _paymentClient = paymentClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var order = await _mediator.Send(new CreateOrderCommand(dto));

        var paymentResponse = await _paymentClient.ProcessPaymentAsync(new ProcessPaymentRequest(order.Id, order.TotalAmount));

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, new { Order = order, Payment = paymentResponse });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _mediator.Send(new GetOrderByIdQuery(id));
        if (order == null) return NotFound();
        return Ok(order);
    }
}