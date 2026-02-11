using FluentValidation;

namespace FamilyVault.API;

/// <summary>
/// Represents MiddlewearGlobalExceaption.
/// </summary>
public class MiddlewearGlobalExceaption
{
    private readonly RequestDelegate _requestDelegate;
    private readonly ILogger<MiddlewearGlobalExceaption> _logger;

    /// <summary>
    /// Initializes a new instance of MiddlewearGlobalExceaption.
    /// </summary>
    public MiddlewearGlobalExceaption(RequestDelegate requestDelegate, ILogger<MiddlewearGlobalExceaption> logger)
    {
        _requestDelegate = requestDelegate;
        _logger = logger;
    }

    /// <summary>
    /// Performs the InvokeAsync operation.
    /// </summary>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _requestDelegate(httpContext); ;
        }
        catch (ValidationException validationException)
        {
            _logger.LogWarning(validationException, "Validation exception occurred");

            var validationErrors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var response = new ApiResponse<Dictionary<string, string[]>>
            {
                IsSuccess = false,
                Message = "Validation failed.",
                ErrorCode = "VALIDATION_ERROR",
                TraceId = httpContext.TraceIdentifier,
                Data = validationErrors
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/json";
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            await httpContext.Response.WriteAsync(jsonResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");

            var response = ApiResponse<string>.Failure(
                message: "An unexpected error occurred.",
                errorCode: "ERR-500",
                traceId: httpContext.TraceIdentifier
            );

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/json";
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            await httpContext.Response.WriteAsync(jsonResponse);
        }

    }
}
