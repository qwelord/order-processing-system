using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OrderService.DataAccess;
using OrderService.DataAccess.Constants;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.Clients;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.Tests;

[TestFixture]
public sealed class PayOrderCommandHandlerTests
{
    private OrderDbContext _dbContext = null!;
    private Mock<IPaymentClient> _paymentClient = null!;
    private PayOrderCommandHandler _handler = null!;
    private Product _product = null!;
    private Order _order = null!;

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new OrderDbContext(options);
        _paymentClient = new Mock<IPaymentClient>();
        _handler = new PayOrderCommandHandler(_dbContext, _paymentClient.Object);

        _product = Product.Create(
            "Laptop",
            "Test product",
            1500m,
            2,
            DateTime.UtcNow);

        _product.TryReserveStock(1);

        _order = Order.Create(
            "Alex",
            "alex@example.com",
            PaymentMethod.Card,
            DateTime.UtcNow);
        _order.AddItem(OrderItem.Create(_product.Id, _product.Name, _product.Price, 1));

        _dbContext.Products.Add(_product);
        _dbContext.Orders.Add(_order);
        await _dbContext.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Handle_ShouldMarkOrderPaid_WhenPaymentSucceeds()
    {
        _paymentClient
            .Setup(client => client.ProcessPaymentAsync(
                It.IsAny<ProcessPaymentRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessPaymentResponse(
                Guid.NewGuid(),
                _order.Id,
                1500m,
                "Completed",
                PaymentMethods.Card,
                "4242",
                DateTime.UtcNow));

        var result = await _handler.Handle(
            new PayOrderCommand(_order.Id, new PayOrderDto("4242")),
            CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(OrderStatus.Paid.ToString()));
        Assert.That((await _dbContext.Orders.SingleAsync()).Status, Is.EqualTo(OrderStatus.Paid));
        Assert.That((await _dbContext.Products.SingleAsync()).StockQuantity, Is.EqualTo(1));
    }

    [Test]
    public async Task Handle_ShouldCancelOrderAndReleaseStock_WhenPaymentFails()
    {
        _paymentClient
            .Setup(client => client.ProcessPaymentAsync(
                It.IsAny<ProcessPaymentRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessPaymentResponse(
                Guid.NewGuid(),
                _order.Id,
                1500m,
                "Failed",
                PaymentMethods.Card,
                "0000",
                DateTime.UtcNow));

        var result = await _handler.Handle(
            new PayOrderCommand(_order.Id, new PayOrderDto("0000")),
            CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(OrderStatus.Cancelled.ToString()));
        Assert.That((await _dbContext.Orders.SingleAsync()).Status, Is.EqualTo(OrderStatus.Cancelled));
        Assert.That((await _dbContext.Products.SingleAsync()).StockQuantity, Is.EqualTo(2));
    }
}
