using FluentValidation;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.WebApi.Validators;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(command => command.OrderDto)
            .SetValidator(new CreateOrderDtoValidator());
    }
}
