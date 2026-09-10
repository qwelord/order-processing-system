using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;
using System.Data;

namespace OrderService.WebApi.UseCases.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponseDto?>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
{
    private readonly OrderDbContext _dbContext;

    public GetOrderByIdQueryHandler(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        const string sql = @"
            SELECT 
                ""Id"", 
                ""CustomerName"", 
                ""TotalAmount"", 
                ""Status"", 
                ""CreatedAt"" 
            FROM ""Orders"" 
            WHERE ""Id"" = @OrderId";

        var orderRaw = await connection.QuerySingleOrDefaultAsync<OrderQueryResult>(sql, new { OrderId = request.OrderId });

        if (orderRaw == null) return null;

        return new OrderResponseDto(
            orderRaw.Id,
            orderRaw.CustomerName,
            orderRaw.TotalAmount,
            ((OrderStatus)orderRaw.Status).ToString(),
            orderRaw.CreatedAt
        );
    }

    private class OrderQueryResult
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}