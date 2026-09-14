using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.Clients;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.Tests;

[TestFixture]
public class CreateOrderCommandHandlerTests
{
    private OrderDbContext _dbContext = null!;
    private Mock<IPaymentClient> _paymentClientMock = null!;
    private CreateOrderCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new OrderDbContext(options);
        _paymentClientMock = new Mock<IPaymentClient>();
        _handler = new CreateOrderCommandHandler(_dbContext, _paymentClientMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCreateOrderFromCatalogAndMarkItPaid()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop",
            Price = 1500m,
            StockQuantity = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        _paymentClientMock
            .Setup(p => p.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
            .ReturnsAsync(new ProcessPaymentResponse(Guid.NewGuid(), Guid.NewGuid(), 1500m, "Completed", "Card", DateTime.UtcNow));

        var result = await _handler.Handle(new CreateOrderCommand(new CreateOrderDto(
            "Алексей",
            "alex@example.com",
            new[] { new CreateOrderItemDto(product.Id, 1) },
            "Card",
            "4242")), CancellationToken.None);

        Assert.That(result.CustomerName, Is.EqualTo("Алексей"));
        Assert.That(result.TotalAmount, Is.EqualTo(1500m));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.Paid.ToString()));
        Assert.That(result.Items, Has.Count.EqualTo(1));

        var savedProduct = await _dbContext.Products.SingleAsync(x => x.Id == product.Id);
        Assert.That(savedProduct.StockQuantity, Is.EqualTo(2));
    }
}
