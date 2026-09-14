using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PaymentService.DataAccess;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.Tests;

[TestFixture]
public class ProcessPaymentCommandHandlerTests
{
    private PaymentDbContext _dbContext = null!;
    private ProcessPaymentCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PaymentDbContext(options);
        _handler = new ProcessPaymentCommandHandler(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Handle_ShouldCompleteCardPaymentAndCreateOutboxMessage()
    {
        var orderId = Guid.NewGuid();

        var result = await _handler.Handle(
            new ProcessPaymentCommand(new ProcessPaymentDto(orderId, 120m, "Card", "4242")),
            CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(PaymentStatus.Completed.ToString()));
        Assert.That(result.CardLast4, Is.EqualTo("4242"));
        Assert.That(await _dbContext.Payments.CountAsync(), Is.EqualTo(1));
        Assert.That(await _dbContext.OutboxMessages.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Handle_ShouldFailCardPayment_WhenDemoCardIs0000()
    {
        var result = await _handler.Handle(
            new ProcessPaymentCommand(new ProcessPaymentDto(Guid.NewGuid(), 120m, "Card", "0000")),
            CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(PaymentStatus.Failed.ToString()));
        Assert.That(await _dbContext.OutboxMessages.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Handle_ShouldReturnExistingPayment_WhenRequestIsRepeated()
    {
        var orderId = Guid.NewGuid();
        var first = await _handler.Handle(
            new ProcessPaymentCommand(new ProcessPaymentDto(orderId, 120m, "Card", "4242")),
            CancellationToken.None);

        var second = await _handler.Handle(
            new ProcessPaymentCommand(new ProcessPaymentDto(orderId, 120m, "Card", "4242")),
            CancellationToken.None);

        Assert.That(second.Id, Is.EqualTo(first.Id));
        Assert.That(await _dbContext.Payments.CountAsync(), Is.EqualTo(1));
    }
}
