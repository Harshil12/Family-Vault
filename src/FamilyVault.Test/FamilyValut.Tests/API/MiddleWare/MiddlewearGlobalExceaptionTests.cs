using FamilyVault.API;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace FamilyVault.Tests.API.Middleware;

public class MiddlewearGlobalExceaptionTests
{
    [Fact]
    public async Task InvokeAsync_ShouldReturnBadRequest_WhenValidationExceptionIsThrown()
    {
        // Arrange
        RequestDelegate next = _ => throw new ValidationException(new[]
        {
            new ValidationFailure("Email", "Invalid email format."),
            new ValidationFailure("Email", "Email is required.")
        });

        var logger = new Mock<ILogger<MiddlewearGlobalExceaption>>();
        var middleware = new MiddlewearGlobalExceaption(next, logger.Object);
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-validation"
        };
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var payload = JsonSerializer.Deserialize<ApiResponse<Dictionary<string, string[]>>>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        payload.Should().NotBeNull();
        payload!.IsSuccess.Should().BeFalse();
        payload.ErrorCode.Should().Be("VALIDATION_ERROR");
        payload.TraceId.Should().Be("trace-validation");
        payload.Data.Should().ContainKey("Email");
        payload.Data!["Email"].Should().Contain("Invalid email format.");
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnInternalServerError_WhenUnhandledExceptionIsThrown()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("boom");
        var logger = new Mock<ILogger<MiddlewearGlobalExceaption>>();
        var middleware = new MiddlewearGlobalExceaption(next, logger.Object);
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-500"
        };
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var payload = JsonSerializer.Deserialize<ApiResponse<string>>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        payload.Should().NotBeNull();
        payload!.IsSuccess.Should().BeFalse();
        payload.ErrorCode.Should().Be("ERR-500");
        payload.TraceId.Should().Be("trace-500");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext_WhenNoExceptionIsThrown()
    {
        // Arrange
        var wasCalled = false;
        RequestDelegate next = context =>
        {
            wasCalled = true;
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        };

        var logger = new Mock<ILogger<MiddlewearGlobalExceaption>>();
        var middleware = new MiddlewearGlobalExceaption(next, logger.Object);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        wasCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }
}
