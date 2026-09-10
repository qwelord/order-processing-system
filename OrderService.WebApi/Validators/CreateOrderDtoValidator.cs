using FluentValidation;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }
}