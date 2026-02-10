
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
                return Results.ValidationProblem(
                    result.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()));
            }
        }

        return await next(context);
    }
}
