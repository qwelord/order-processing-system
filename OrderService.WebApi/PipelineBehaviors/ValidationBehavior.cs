using FluentValidation;
using MediatR;

namespace OrderService.WebApi.PipelineBehaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var properties = request.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var propValue = prop.GetValue(request);
            if (propValue == null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(prop.PropertyType);
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                var context = new ValidationContext<object>(propValue);
                var result = await validator.ValidateAsync(context, cancellationToken);

                if (!result.IsValid)
                {
                    throw new ValidationException(result.Errors);
                }
            }
        }

        return await next();
    }
}