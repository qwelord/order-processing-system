using FluentValidation;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.Validators;

public class PayOrderDtoValidator : AbstractValidator<PayOrderDto>
{
    public PayOrderDtoValidator()
    {
        RuleFor(payment => payment.CardLast4)
            .NotEmpty()
            .Matches("^[0-9]{4}$")
            .WithMessage("CardLast4 must contain exactly four digits.");
    }
}
