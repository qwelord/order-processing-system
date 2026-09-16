using FluentValidation;
using PaymentService.DataAccess.Constants;
using PaymentService.WebApi.DTOs;

namespace PaymentService.WebApi.Validators;

public sealed class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentDtoValidator()
    {
        RuleFor(payment => payment.OrderId).NotEmpty();
        RuleFor(payment => payment.Amount).GreaterThan(0);
        RuleFor(payment => payment.PaymentMethod)
            .Must(PaymentMethods.IsSupported)
            .WithMessage($"Payment method must be {PaymentMethods.Card} or {PaymentMethods.CashOnDelivery}.");

        When(payment => payment.PaymentMethod == PaymentMethods.Card, () =>
        {
            RuleFor(payment => payment.CardLast4)
                .NotEmpty()
                .Matches($"^[0-9]{{{PaymentLimits.CardLast4Length}}}$")
                .WithMessage($"Card last four digits must contain exactly {PaymentLimits.CardLast4Length} digits.");
        });
    }
}
