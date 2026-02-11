using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity.Data;

namespace FamilyVault.API.EndPoints.Login;

/// <summary>
/// Represents LoginEndpoints.
/// </summary>
public static class LoginEndpoints
{
    /// <summary>
    /// Performs the MapLoginPoints operation.
    /// </summary>
    public static void MapLoginPoints(this WebApplication app)
    {
        app.MapPost("/login", async (
        LoginRequest request,
        IAuthService authService, 
        CancellationToken cancellationToken) =>
            {
                var token = await authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

                if (token == null)
                    return Results.Unauthorized();
                else
                    return Results.Ok(new { token });
            });

        app.MapPost("/register", async (
            CreateUserRequest createUserRequest,
            IUserService userService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;

            var createdUser = await userService.RegisterUserAsync(createUserRequest, cancellationToken);

            return Results.Created($"/user/{createdUser.Id}",
                ApiResponse<UserDto>.Success(createdUser, "User has been successfully registered.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateUserRequest>>();
    }
}
