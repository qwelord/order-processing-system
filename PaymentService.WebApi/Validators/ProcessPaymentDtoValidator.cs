using FluentValidation;
using PaymentService.WebApi.DTOs;

namespace PaymentService.WebApi.Validators;

public class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentDtoValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).Must(x => x is "Card" or "CashOnDelivery");
        When(x => x.PaymentMethod == "Card", () =>
            RuleFor(x => x.CardLast4)
                .NotEmpty()
                .Matches("^[0-9]{4}$")
                .WithMessage("CardLast4 must contain exactly four digits."));
    }
}
