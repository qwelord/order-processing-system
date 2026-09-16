using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.WebApi.DTOs;
using PaymentService.WebApi.UseCases.Commands;
using PaymentService.WebApi.UseCases.Queries;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PaymentResponseDto>>> GetPayments(
        [FromQuery] Guid? orderId,
        CancellationToken cancellationToken)
    {
        var payments = await _mediator.Send(new ListPaymentsQuery(orderId), cancellationToken);
        return Ok(payments);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponseDto>> GetPayment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await _mediator.Send(new GetPaymentByIdQuery(id), cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentResponseDto>> ProcessPayment(
        [FromBody] ProcessPaymentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ProcessPaymentCommand(dto), cancellationToken);
        return Ok(result);
    }
}
