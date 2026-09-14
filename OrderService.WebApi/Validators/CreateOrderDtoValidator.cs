using FluentValidation;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(order => order.CustomerName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(order => order.CustomerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(order => order.Items)
            .NotEmpty();

        RuleForEach(order => order.Items)
            .ChildRules(item =>
            {
                item.RuleFor(value => value.ProductId).NotEmpty();
                item.RuleFor(value => value.Quantity).InclusiveBetween(1, 100);
            });

        RuleFor(order => order.PaymentMethod)
            .Must(method => method is "Card" or "CashOnDelivery")
            .WithMessage("Payment method must be Card or CashOnDelivery.");
    }
}
