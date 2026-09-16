using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderService.DataAccess;
using OrderService.DataAccess.Constants;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.Tests;

[TestFixture]
public sealed class CreateOrderCommandHandlerTests
{
    private OrderDbContext _dbContext = null!;
    private CreateOrderCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new OrderDbContext(options);
        _handler = new CreateOrderCommandHandler(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCreatePendingOrderAndReserveStock()
    {
        var product = Product.Create(
            "Laptop",
            "Test product",
            1500m,
            3,
            DateTime.UtcNow);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var result = await _handler.Handle(
            new CreateOrderCommand(new CreateOrderDto(
                "Alex",
                "alex@example.com",
                new[] { new CreateOrderItemDto(product.Id, 1) },
                PaymentMethods.Card)),
            CancellationToken.None);

        Assert.That(result.CustomerName, Is.EqualTo("Alex"));
        Assert.That(result.TotalAmount, Is.EqualTo(1500m));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.PendingPayment.ToString()));
        Assert.That(result.Items, Has.Count.EqualTo(1));

        var savedProduct = await _dbContext.Products.SingleAsync(productEntity => productEntity.Id == product.Id);
        Assert.That(savedProduct.StockQuantity, Is.EqualTo(2));
    }
}
