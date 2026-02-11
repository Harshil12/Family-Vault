
using FluentValidation;

namespace FamilyVault.API.EndPoints;

/// <summary>
/// Represents ValidationFilter.
/// </summary>
public class ValidationFilter<T> : IEndpointFilter
{
    /// <summary>
    /// Performs the InvokeAsync operation.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices
           .GetService<IValidator<T>>();

        if (validator is not null)
        {
            var model = context.Arguments.OfType<T>().First();
            var result = await validator.ValidateAsync(model);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        return await next(context);
    }
}
