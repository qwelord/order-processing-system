using AutoMapper;
using MediatR;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public record CreateOrderCommand(CreateOrderDto OrderDto) : IRequest<OrderResponseDto>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(OrderDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = _mapper.Map<Order>(request.OrderDto);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderResponseDto>(order);
    }
}