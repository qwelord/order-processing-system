using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess;
using PaymentService.WebApi.DTOs;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly PaymentDbContext _db;

    public PaymentsController(IMediator mediator, PaymentDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments([FromQuery] Guid? orderId, CancellationToken cancellationToken)
    {
        var query = _db.Payments.AsNoTracking();
        if (orderId.HasValue) query = query.Where(x => x.OrderId == orderId.Value);

        var payments = await query
            .OrderByDescending(x => x.ProcessedAt)
            .Take(200)
            .Select(x => new PaymentResponseDto(x.Id, x.OrderId, x.Amount, x.Status.ToString(), x.PaymentMethod, x.CardLast4, x.ProcessedAt))
            .ToListAsync(cancellationToken);

        return Ok(payments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPayment(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (payment is null) return NotFound();
        return Ok(new PaymentResponseDto(payment.Id, payment.OrderId, payment.Amount, payment.Status.ToString(), payment.PaymentMethod, payment.CardLast4, payment.ProcessedAt));
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ProcessPaymentCommand(dto), cancellationToken);
        return Ok(result);
    }
}
