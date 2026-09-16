using FluentValidation;
using OrderService.DataAccess.Constants;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.Validators;

public sealed class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(order => order.CustomerName)
            .NotEmpty()
            .MaximumLength(OrderLimits.CustomerNameMaxLength);

        RuleFor(order => order.CustomerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(OrderLimits.CustomerEmailMaxLength);

        RuleFor(order => order.Items)
            .NotEmpty();

        RuleForEach(order => order.Items)
            .SetValidator(new CreateOrderItemValidator());

        RuleFor(order => order.PaymentMethod)
            .Must(PaymentMethods.IsSupported)
            .WithMessage($"Payment method must be {PaymentMethods.Card} or {PaymentMethods.CashOnDelivery}.");
    }

    private sealed class CreateOrderItemValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemValidator()
        {
            RuleFor(item => item.ProductId).NotEmpty();
            RuleFor(item => item.Quantity)
                .InclusiveBetween(OrderLimits.ProductQuantityMin, OrderLimits.ProductQuantityMax);
        }
    }
}
