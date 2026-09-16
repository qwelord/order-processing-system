using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.Clients;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.UseCases.Commands;

public sealed record PayOrderCommand(Guid OrderId, PayOrderDto Payment) : IRequest<OrderResponseDto>;

public sealed class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, OrderResponseDto>
{
    private readonly OrderDbContext _db;
    private readonly IPaymentClient _paymentClient;

    public PayOrderCommandHandler(OrderDbContext db, IPaymentClient paymentClient)
    {
        _db = db;
        _paymentClient = paymentClient;
    }

    public async Task<OrderResponseDto> Handle(
        PayOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == request.OrderId, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order was not found.");

        if (order.Status == OrderStatus.Paid)
        {
            throw new ValidationException([
                new ValidationFailure(
                    nameof(PayOrderCommand.OrderId),
                    "Order has already been paid.")
            ]);
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new ValidationException([
                new ValidationFailure(
                    nameof(PayOrderCommand.OrderId),
                    "Cancelled orders cannot be paid.")
            ]);
        }

        if (!order.UsesCardPayment())
        {
            throw new ValidationException([
                new ValidationFailure(
                    "PaymentMethod",
                    "Only card orders can be paid online.")
            ]);
        }

        ProcessPaymentResponse payment;

        try
        {
            payment = await _paymentClient.ProcessPaymentAsync(
                new ProcessPaymentRequest(
                    order.Id,
                    order.TotalAmount,
                    order.PaymentMethod.ToString(),
                    request.Payment.CardLast4),
                cancellationToken);
        }
        catch
        {
            var existingPayment = await FindExistingPaymentAsync(order.Id, cancellationToken);

            if (existingPayment?.Status == "Completed")
                return await MarkPaidAsync(order, cancellationToken);

            if (existingPayment?.Status == "Failed")
                return await CancelAndReleaseStockAsync(order, cancellationToken);

            throw new InvalidOperationException("Payment result could not be confirmed.");
        }

        if (payment.Status == "Completed")
            return await MarkPaidAsync(order, cancellationToken);

        if (payment.Status == "Failed")
            return await CancelAndReleaseStockAsync(order, cancellationToken);

        throw new ValidationException(
            "Payment service returned an unsupported status.");
    }

    private async Task<ProcessPaymentResponse?> FindExistingPaymentAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var payments = await _paymentClient.GetPaymentsAsync(
                orderId,
                cancellationToken);

            return payments
                .OrderByDescending(payment => payment.ProcessedAt)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private async Task<OrderResponseDto> MarkPaidAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        order.MarkPaid();

        await _db.SaveChangesAsync(cancellationToken);

        return OrderResponseDto.FromEntity(order);
    }

    private async Task<OrderResponseDto> CancelAndReleaseStockAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        var productIds = order.Items
            .Select(item => item.ProductId)
            .ToArray();

        var products = await _db.Products
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        foreach (var item in order.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
                product.ReleaseStock(item.Quantity);
        }

        order.Cancel();

        await _db.SaveChangesAsync(cancellationToken);

        return OrderResponseDto.FromEntity(order);
    }
}