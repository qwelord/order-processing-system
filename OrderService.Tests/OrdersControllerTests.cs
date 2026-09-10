using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OrderService.WebApi.Clients;
using OrderService.WebApi.Controllers;
using OrderService.WebApi.DTOs;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.Tests;

[TestFixture]
public class OrdersControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private Mock<IPaymentClient> _paymentClientMock = null!;
    private OrdersController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>();
        _paymentClientMock = new Mock<IPaymentClient>();
        _controller = new OrdersController(_mediatorMock.Object, _paymentClientMock.Object);
    }

    [Test]
    public async Task CreateOrder_ShouldReturnCreatedAtActionWithOrderAndPayment()
    {
        var createDto = new CreateOrderDto("Алексей", 2000m);
        var orderResponse = new OrderResponseDto(Guid.NewGuid(), "Алексей", 2000m, "Created", DateTime.UtcNow);
        var paymentResponse = new ProcessPaymentResponse(Guid.NewGuid(), orderResponse.Id, 2000m, "Completed", DateTime.UtcNow);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderResponse);

        _paymentClientMock.Setup(p => p.ProcessPaymentAsync(It.IsAny<ProcessPaymentRequest>()))
            .ReturnsAsync(paymentResponse);

        var actionResult = await _controller.CreateOrder(createDto);

        var createdResult = actionResult as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult!.StatusCode, Is.EqualTo(201));

        _paymentClientMock.Verify(p => p.ProcessPaymentAsync(It.Is<ProcessPaymentRequest>(
            req => req.OrderId == orderResponse.Id && req.Amount == orderResponse.TotalAmount)), Times.Once);
    }
}