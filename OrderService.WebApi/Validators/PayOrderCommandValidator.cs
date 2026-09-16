using FluentValidation;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.WebApi.Validators;

public sealed class PayOrderCommandValidator : AbstractValidator<PayOrderCommand>
{
    public PayOrderCommandValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.Payment)
            .SetValidator(new PayOrderDtoValidator());
    }
}
