using FluentValidation;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).InclusiveBetween(1, 100);
        });
        RuleFor(x => x.PaymentMethod).Must(value => value is "Card" or "CashOnDelivery");
        When(x => x.PaymentMethod == "Card", () =>
            RuleFor(x => x.CardLast4).Matches("^[0-9]{4}$").NotEmpty());
    }
}
