using FamilyVault.Application.Services;
using Microsoft.AspNetCore.Identity.Data;

namespace FamilyVault.API.EndPoints.Login;

public static class LoginEndpoints
{
    public static void MapLoginPoints(this WebApplication app)
    {
        app.MapPost("/login", (
        LoginRequest request,
        JwtTokenService tokenService) =>
            {
                // 1️⃣ Validate user credentials (mocked)
                if (request.Email != "admin@fv.com" || request.Password != "password")
                    return Results.Unauthorized();

                // 2️⃣ Generate token
                var token = tokenService.GenerateToken(Guid.NewGuid(), request.Email);

                return Results.Ok(new { token });
            });
    }
}
