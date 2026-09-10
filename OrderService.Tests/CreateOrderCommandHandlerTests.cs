using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderService.DataAccess;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.Mappings;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.Tests;

[TestFixture]
public class CreateOrderCommandHandlerTests
{
    private OrderDbContext _dbContext = null!;
    private IMapper _mapper = null!;
    private CreateOrderCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new OrderDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _handler = new CreateOrderCommandHandler(_dbContext, _mapper);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task Handle_ShouldSaveOrderToDatabaseAndReturnMappedDto()
    {
        var dto = new CreateOrderDto("Алексей", 1500m);
        var command = new CreateOrderCommand(dto);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.CustomerName, Is.EqualTo("Алексей"));
        Assert.That(result.TotalAmount, Is.EqualTo(1500m));
        Assert.That(result.Status, Is.EqualTo(OrderStatus.Created.ToString()));

        var savedOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == result.Id);
        Assert.That(savedOrder, Is.Not.Null);
        Assert.That(savedOrder!.CustomerName, Is.EqualTo("Алексей"));
        Assert.That(savedOrder.TotalAmount, Is.EqualTo(1500m));
    }
}