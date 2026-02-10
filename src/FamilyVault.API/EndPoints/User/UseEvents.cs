using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Services;
using System.Security.Claims;

namespace FamilyVault.API.EndPoints.User;

/// <summary>
/// Represents MapAllEndpoints.
/// </summary>
public static class MapAllEndpoints
{
    /// <summary>
    /// Performs the MapUserEndPoints operation.
    /// </summary>
    public static void MapUserEndPoints(this WebApplication app)
    {
        var familyGroup = app.MapGroup("/User").RequireAuthorization();

        familyGroup.MapGet("/", async (IUserService userService, HttpContext httpContext, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("GetUsersEndpoint");

            var usersDetails = await userService.GetUserAsync(cancellationToken);

            if (usersDetails is null || !usersDetails.Any())
            {
                logger.LogWarning($"No user found. TraceId: {traceId}");

                return Results.Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(Array.Empty<UserDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(usersDetails, string.Empty, traceId));

        });

        familyGroup.MapGet("/{id:Guid}", async (Guid id, IUserService userService, HttpContext httpContext, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("GetUserByIdEndpoint");

            var userDetail = await userService.GetUserByIdAsync(id, cancellationToken);

            if (userDetail is null)
            {
                logger.LogWarning($"No user found for id - {id}. TraceId: {traceId}");

                return Results.NotFound(ApiResponse<UserDto>.Failure(
                        message: "No user found for given id",
                        errorCode: "USER_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<UserDto>.Success(userDetail, string.Empty, traceId));

        });

        familyGroup.MapDelete("/{id:guid}", async (Guid id, IUserService userService, HttpContext httpContext, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;

            await userService.DeleteUserByIdAsync(id, userId, cancellationToken);

            return Results.Ok(ApiResponse<UserDto>.Success(null, "User has been successfully deleted.", traceId));

        });

        familyGroup.MapPost("/user", async (CreateUserRequest createUserRequest, IUserService userService, HttpContext httpContext, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;

            var createdUser = await userService.CreateUserAsync(createUserRequest, userId, cancellationToken);

            return Results.Created($"/user/{createdUser.Id}",
                    ApiResponse<UserDto>.Success(createdUser, "User has been successfully createdUser.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateUserRequest>>();

        familyGroup.MapPut("/user/{id:guid}", async (UpdateUserRequest updateUserRequest, 
            IUserService userService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;

            var updatedUser = await userService.UpdateUserAsync(updateUserRequest, userId, cancellationToken);

            return Results.Ok(ApiResponse<UserDto>.Success(updatedUser, "Update has been successfully updatedUser.", traceId));

        }).AddEndpointFilter<ValidationFilter<UpdateUserRequest>>();
    }
}
