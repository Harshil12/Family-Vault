using FamilyVault.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity.Data;

namespace FamilyVault.API.EndPoints.Login;

public static class LoginEndpoints
{
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
    }
}
