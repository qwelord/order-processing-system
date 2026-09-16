using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess;
using PaymentService.WebApi.DTOs;

namespace PaymentService.WebApi.UseCases.Queries;

public sealed record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentResponseDto?>;

public sealed class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentResponseDto?>
{
    private readonly PaymentDbContext _db;

    public GetPaymentByIdQueryHandler(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task<PaymentResponseDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.Payments
            .AsNoTracking()
            .Where(payment => payment.Id == request.Id)
            .Select(payment => new PaymentResponseDto(
                payment.Id,
                payment.OrderId,
                payment.Amount,
                payment.Status.ToString(),
                payment.PaymentMethod.ToString(),
                payment.CardLast4,
                payment.ProcessedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
