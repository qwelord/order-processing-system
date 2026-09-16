using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OrderService.DataAccess.Constants;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.Tests;

[TestFixture]
public sealed class OrdersControllerTests
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
        var createDto = new CreateOrderDto(
            "Alex",
            "alex@example.com",
            new[] { new CreateOrderItemDto(Guid.NewGuid(), 1) },
            PaymentMethods.Card);

        var orderResponse = new OrderResponseDto(
            Guid.NewGuid(),
            "Alex",
            "alex@example.com",
            2000m,
            OrderStatus.PendingPayment.ToString(),
            PaymentMethods.Card,
            DateTime.UtcNow,
            Array.Empty<OrderItemResponseDto>());

        _mediatorMock
            .Setup(mediator => mediator.Send(
                It.IsAny<CreateOrderCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderResponse);

        var actionResult = await _controller.CreateOrder(createDto, CancellationToken.None);

        var createdResult = actionResult.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
    }

    [Test]
    public async Task PayOrder_ShouldReturnOrderFromMediator()
    {
        var orderId = Guid.NewGuid();
        var order = new OrderResponseDto(
            orderId,
            "Alex",
            "alex@example.com",
            2000m,
            OrderStatus.Paid.ToString(),
            PaymentMethods.Card,
            DateTime.UtcNow,
            Array.Empty<OrderItemResponseDto>());

        _mediatorMock
            .Setup(mediator => mediator.Send(
                It.IsAny<PayOrderCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var actionResult = await _controller.PayOrder(
            orderId,
            new PayOrderDto("4242"),
            CancellationToken.None);

        var okResult = actionResult.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.SameAs(order));
    }
}
