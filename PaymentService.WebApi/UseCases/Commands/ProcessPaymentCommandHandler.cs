using System.Text.Json;
using AutoMapper;
using MediatR;
using PaymentService.DataAccess;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;
using PaymentService.WebApi.Services;

namespace PaymentService.WebApi.UseCases.Commands;

public record ProcessPaymentCommand(ProcessPaymentDto PaymentDto) : IRequest<PaymentResponseDto>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponseDto>
{
    private readonly PaymentDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProcessPaymentCommandHandler(PaymentDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PaymentResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = _mapper.Map<Payment>(request.PaymentDto);
        payment.Status = PaymentStatus.Completed;
        payment.ProcessedAt = DateTime.UtcNow;

        _dbContext.Payments.Add(payment);

        var eventMessage = new PaymentCompletedEvent(payment.OrderId, payment.Id, payment.Amount, "Completed", DateTime.UtcNow);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(PaymentCompletedEvent),
            Content = JsonSerializer.Serialize(eventMessage),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.OutboxMessages.Add(outboxMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PaymentResponseDto>(payment);
    }
}