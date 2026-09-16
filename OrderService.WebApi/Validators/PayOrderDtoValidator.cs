using FluentValidation;
using OrderService.DataAccess.Constants;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.Validators;

public sealed class PayOrderDtoValidator : AbstractValidator<PayOrderDto>
{
    public PayOrderDtoValidator()
    {
        RuleFor(payment => payment.CardLast4)
            .NotEmpty()
            .Matches($"^[0-9]{{{OrderLimits.CardLast4Length}}}$")
            .WithMessage($"Card last four digits must contain exactly {OrderLimits.CardLast4Length} digits.");
    }
}
