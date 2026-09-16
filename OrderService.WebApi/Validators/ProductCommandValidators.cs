using FluentValidation;
using OrderService.DataAccess.Constants;
using OrderService.WebApi.UseCases.Commands;

namespace OrderService.WebApi.Validators;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.ProductDto.Name)
            .NotEmpty()
            .MaximumLength(OrderLimits.ProductNameMaxLength);

        RuleFor(command => command.ProductDto.Description)
            .MaximumLength(OrderLimits.ProductDescriptionMaxLength);

        RuleFor(command => command.ProductDto.Price)
            .GreaterThan(0);

        RuleFor(command => command.ProductDto.StockQuantity)
            .GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.ProductDto.Name)
            .NotEmpty()
            .MaximumLength(OrderLimits.ProductNameMaxLength);

        RuleFor(command => command.ProductDto.Description)
            .MaximumLength(OrderLimits.ProductDescriptionMaxLength);

        RuleFor(command => command.ProductDto.Price)
            .GreaterThan(0);

        RuleFor(command => command.ProductDto.StockQuantity)
            .GreaterThanOrEqualTo(0);
    }
}
