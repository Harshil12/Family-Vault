using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.API.EndPoints.User;

public static class MapAllEndpoints
{
    public static void MapUserEndPoints(this WebApplication app)
    {
        var familyGroup = app.MapGroup("/User");

        familyGroup.MapGet("/", async (IUserService userService, HttpContext httpContext, ILoggerFactory loggerFactory) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var result = await userService.GetUserAsync();
            var logger = loggerFactory.CreateLogger("GetUsersEndpoint");

            if (result is null || !result.Any())
            {
                logger.LogWarning($"No user found. TraceId: {traceId}");
                return Results.Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(Array.Empty<UserDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(result, string.Empty, traceId));
        });

        familyGroup.MapGet("/{id:Guid}", async (Guid id, IUserService userService, HttpContext httpContext, ILoggerFactory loggerFactory) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var result = await userService.GetUserByIdAsync(id);
            var logger = loggerFactory.CreateLogger("GetUserByIdEndpoint");
            if (result is null)
            {
                logger.LogWarning($"No user found for id - {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<UserDto>.Failure(
                        message: "No user found for given id",
                        errorCode: "USER_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<UserDto>.Success(result, string.Empty, traceId));
        });

        familyGroup.MapDelete("/{id:guid}", async (Guid id, IUserService userService, HttpContext httpContext) =>
        {
            var traceId = httpContext.TraceIdentifier;
            await userService.DeleteUserByIdAsync(id);

            return Results.Ok(ApiResponse<UserDto>.Success(null, "User has been successfully deleted.", traceId));
        });

        familyGroup.MapPost("/user", async (CreateUserRequest createUserRequest, IUserService userService, HttpContext httpContext) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var created = await userService.CreateUserAsync(createUserRequest);

            return Results.Created($"/user/{created.Id}",
                    ApiResponse<UserDto>.Success(created, "User has been successfully created.", traceId));
        });

        familyGroup.MapPut("/user/{id:guid}", async (UpdateUserRequest updateUserRequest, IUserService userService, HttpContext httpContext) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var updated = await userService.UpdateuUerAsync(updateUserRequest);

            return Results.Ok(ApiResponse<UserDto>.Success(updated, "Update has been successfully updated.", traceId));
        });
    }
}
