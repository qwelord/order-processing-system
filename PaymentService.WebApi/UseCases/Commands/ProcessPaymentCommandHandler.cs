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
    private readonly IKafkaProducerService _kafkaProducer;

    public ProcessPaymentCommandHandler(PaymentDbContext dbContext, IMapper mapper, IKafkaProducerService kafkaProducer)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<PaymentResponseDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = _mapper.Map<Payment>(request.PaymentDto);

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _kafkaProducer.PublishPaymentCompletedAsync(payment.OrderId, payment.Id, payment.Amount);

        return _mapper.Map<PaymentResponseDto>(payment);
    }
}