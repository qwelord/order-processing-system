using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OrderService.WebApi.Controllers;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.Tests;

[TestFixture]
public class OrdersControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private OrdersController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new OrdersController(_mediatorMock.Object);
    }

    [Test]
    public async Task CreateOrder_ShouldReturnCreatedAtActionWithOrder()
    {
        var createDto = new CreateOrderDto("Алексей", 2000m);
        var orderResponse = new OrderResponseDto(Guid.NewGuid(), "Алексей", 2000m, "Paid", DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderResponse);

        var actionResult = await _controller.CreateOrder(createDto);

        var createdResult = actionResult as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.StatusCode, Is.EqualTo(201));
    }
}