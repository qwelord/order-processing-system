using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess;
using PaymentService.DataAccess.Constants;
using PaymentService.WebApi.DTOs;

namespace PaymentService.WebApi.UseCases.Queries;

public sealed record ListPaymentsQuery(Guid? OrderId) : IRequest<IReadOnlyCollection<PaymentResponseDto>>;

public sealed class ListPaymentsQueryHandler : IRequestHandler<ListPaymentsQuery, IReadOnlyCollection<PaymentResponseDto>>
{
    private readonly PaymentDbContext _db;

    public ListPaymentsQueryHandler(PaymentDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<PaymentResponseDto>> Handle(
        ListPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Payments.AsNoTracking();

        if (request.OrderId.HasValue)
            query = query.Where(payment => payment.OrderId == request.OrderId.Value);

        return await query
            .OrderByDescending(payment => payment.ProcessedAt)
            .Take(PaymentLimits.PaymentHistoryPageSize)
            .Select(payment => new PaymentResponseDto(
                payment.Id,
                payment.OrderId,
                payment.Amount,
                payment.Status.ToString(),
                payment.PaymentMethod.ToString(),
                payment.CardLast4,
                payment.ProcessedAt))
            .ToListAsync(cancellationToken);
    }
}
