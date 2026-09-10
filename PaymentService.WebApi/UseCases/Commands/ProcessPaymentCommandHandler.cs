using AutoMapper;
using MediatR;
using PaymentService.DataAccess;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;

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

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PaymentResponseDto>(payment);
    }
}