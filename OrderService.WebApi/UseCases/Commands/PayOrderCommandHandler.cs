using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.Clients;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public record PayOrderCommand(Guid OrderId, PayOrderDto Payment) : IRequest<OrderResponseDto>;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _db;
    private readonly IPaymentClient _paymentClient;

    public PayOrderCommandHandler(OrderDbContext db, IPaymentClient paymentClient)
    {
        _db = db;
        _paymentClient = paymentClient;
    }

    public async Task<OrderResponseDto> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.Id == request.OrderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order was not found.");

        if (order.Status == OrderStatus.Paid)
            throw new ValidationException(new[] { new ValidationFailure("Order", "Order has already been paid.") });

        if (order.Status == OrderStatus.Cancelled)
            throw new ValidationException(new[] { new ValidationFailure("Order", "Cancelled orders cannot be paid.") });

        if (order.PaymentMethod != "Card")
            throw new ValidationException(new[]
            {
                new ValidationFailure("PaymentMethod", "Only card orders can be paid online.")
            });

        if (string.IsNullOrWhiteSpace(request.Payment.CardLast4) ||
            request.Payment.CardLast4.Length != 4 ||
            !request.Payment.CardLast4.All(char.IsDigit))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("CardLast4", "Enter the last four digits of the card number.")
            });
        }

        ProcessPaymentResponse payment;

        try
        {
            payment = await _paymentClient.ProcessPaymentAsync(
                new ProcessPaymentRequest(
                    order.Id,
                    order.TotalAmount,
                    order.PaymentMethod,
                    request.Payment.CardLast4));
        }
        catch
        {
            var existingPayment = await FindExistingPaymentAsync(order.Id, cancellationToken);

            if (existingPayment?.Status == "Completed")
            {
                order.Status = OrderStatus.Paid;
                await _db.SaveChangesAsync(cancellationToken);
                return ToResponse(order);
            }

            if (existingPayment?.Status == "Failed")
                return await CancelAndReleaseStockAsync(order, cancellationToken);

            throw;
        }

        if (payment.Status == "Completed")
        {
            order.Status = OrderStatus.Paid;
            await _db.SaveChangesAsync(cancellationToken);
            return ToResponse(order);
        }

        return await CancelAndReleaseStockAsync(order, cancellationToken);
    }

    private async Task<ProcessPaymentResponse?> FindExistingPaymentAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var payments = await _paymentClient.GetPaymentsAsync(orderId);
            return payments.OrderByDescending(item => item.ProcessedAt).FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private async Task<OrderResponseDto> CancelAndReleaseStockAsync(Order order, CancellationToken cancellationToken)
    {
        var productIds = order.Items.Select(item => item.ProductId).ToArray();
        var products = await _db.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
                product.StockQuantity += item.Quantity;
        }

        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(order);
    }

    private static OrderResponseDto ToResponse(Order order) => new(
        order.Id,
        order.CustomerName,
        order.CustomerEmail,
        order.TotalAmount,
        order.Status.ToString(),
        order.PaymentMethod,
        order.CreatedAt,
        order.Items.Select(item => new OrderItemResponseDto(
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity,
            item.LineTotal)).ToList());
}
