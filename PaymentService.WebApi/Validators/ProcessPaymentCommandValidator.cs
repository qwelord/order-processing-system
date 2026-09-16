using FluentValidation;
using PaymentService.WebApi.UseCases.Commands;

namespace PaymentService.WebApi.Validators;

public sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(command => command.PaymentDto)
            .SetValidator(new ProcessPaymentDtoValidator());
    }
}
