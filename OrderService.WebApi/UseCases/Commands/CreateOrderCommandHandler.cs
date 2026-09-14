using AutoMapper;
using MediatR;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.Clients;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public record CreateOrderCommand(CreateOrderDto OrderDto) : IRequest<OrderResponseDto>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IPaymentClient _paymentClient;

    public CreateOrderCommandHandler(OrderDbContext dbContext, IMapper mapper, IPaymentClient paymentClient)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _paymentClient = paymentClient;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = _mapper.Map<Order>(request.OrderDto);
        order.Status = OrderStatus.PendingPayment;

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var paymentResponse = await _paymentClient.ProcessPaymentAsync(new ProcessPaymentRequest(order.Id, order.TotalAmount));

            if (paymentResponse.Status == "Completed")
            {
                order.Status = OrderStatus.Paid;
            }
            else
            {
                await CompensateOrderAsync(order, cancellationToken);
            }
        }
        catch (Exception)
        {
            await CompensateOrderAsync(order, cancellationToken);
            throw;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrderResponseDto>(order);
    }

    private async Task CompensateOrderAsync(Order order, CancellationToken cancellationToken)
    {
        order.Status = OrderStatus.Cancelled;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}