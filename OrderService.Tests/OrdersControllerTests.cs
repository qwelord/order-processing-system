using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OrderService.DataAccess.Entities;
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
        _controller = new OrdersController(_mediatorMock.Object, null!);
    }

    [Test]
    public async Task CreateOrder_ShouldReturnCreatedAtActionWithOrder()
    {
        var createDto = new CreateOrderDto(
            "Алексей",
            "alex@example.com",
            new[] { new CreateOrderItemDto(Guid.NewGuid(), 1) },
            "Card");

        var orderResponse = new OrderResponseDto(
            Guid.NewGuid(),
            "Алексей",
            "alex@example.com",
            2000m,
            OrderStatus.PendingPayment.ToString(),
            "Card",
            DateTime.UtcNow,
            Array.Empty<OrderItemResponseDto>());

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderResponse);

        var actionResult = await _controller.CreateOrder(createDto, CancellationToken.None);

        var createdResult = actionResult as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.StatusCode, Is.EqualTo(201));
    }

    [Test]
    public async Task PayOrder_ShouldReturnOrderFromMediator()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderResponseDto(
            orderId,
            "Алексей",
            "alex@example.com",
            2000m,
            OrderStatus.Paid.ToString(),
            "Card",
            DateTime.UtcNow,
            Array.Empty<OrderItemResponseDto>());

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<PayOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _controller.PayOrder(
            orderId,
            new PayOrderDto("4242"),
            CancellationToken.None);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(order));
    }
}
