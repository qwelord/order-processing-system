using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.WebApi.Commands;
using PaymentService.WebApi.DTOs;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        var result = await _mediator.Send(new ProcessPaymentCommand(dto));
        return Ok(result);
    }
}