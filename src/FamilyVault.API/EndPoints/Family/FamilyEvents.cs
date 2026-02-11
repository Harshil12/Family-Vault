using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Services;
using System.Security.Claims;

namespace FamilyVault.API.EndPoints.Family;

/// <summary>
/// Represents FamilymemberEvents.
/// </summary>
public static class FamilymemberEvents
{
    /// <summary>
    /// Performs the MapFamilyEndPoints operation.
    /// </summary>
    public static void MapFamilyEndPoints(this WebApplication app)
    {
        var familyGroup = app.MapGroup("/family/{userId:Guid}").RequireAuthorization();

        familyGroup.MapGet("/", async (Guid userId, IFamilyService familyService, HttpContext httpContext, ILoggerFactory loggerFactory, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken) =>
        {
            var authUserId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (authUserId != userId)
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FamilyEndpoints");

            var familyDetails = await familyService.GetFamilyByUserIdAsync(userId, cancellationToken);

            if (familyDetails is null || !familyDetails.Any())
            {
                logger.LogWarning($"No family found. TraceId: {traceId}");

                return Results.Ok(ApiResponse<IReadOnlyList<FamilyDto>>.Success(Array.Empty<FamilyDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<FamilyDto>>.Success(familyDetails, string.Empty, traceId));

        });

        familyGroup.MapGet("/{id:Guid}", async (Guid userId, Guid id, IFamilyService familyService, HttpContext httpContext, ILoggerFactory loggerFactory, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken) =>
        {
            var authUserId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (authUserId != userId)
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FamilyEndpoints");

            var familyDetail = await familyService.GetFamilyByIdAsync(id, cancellationToken);

            if (familyDetail is null || familyDetail.UserId != authUserId)
            {
                logger.LogWarning($"No family found for id - {id}. TraceId: {traceId}");

                return Results.NotFound(ApiResponse<FamilyDto>.Failure(
                        message: "No family found for given id",
                        errorCode: "FAMILY_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<FamilyDto>.Success(familyDetail, string.Empty, traceId));

        });

        familyGroup.MapDelete("/{id:guid}", async (Guid userId, Guid id, IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var authUserId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (authUserId != userId)
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var familyDetail = await familyService.GetFamilyByIdAsync(id, cancellationToken);
            if (familyDetail is null || familyDetail.UserId != authUserId)
            {
                return Results.Forbid();
            }

            await familyService.DeleteFamilyByIdAsync(id, authUserId, cancellationToken);
            await auditService.LogAsync(
                authUserId,
                "Delete",
                "Family",
                id,
                $"Deleted family {id}",
                id,
                null,
                null,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Ok(ApiResponse<FamilyDto>.Success(null, "Family has been successfully deleted.", traceId));

        });

        familyGroup.MapPost("/family", async (Guid userId, CreateFamilyRequest createFamilyRequest,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var authUserId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (authUserId != userId)
            {
                return Results.Forbid();
            }
            var traceId = httpContext.TraceIdentifier;

            var createdFamily = await familyService.CreateFamilyAsync(createFamilyRequest, authUserId, cancellationToken);
            await auditService.LogAsync(
                authUserId,
                "Create",
                "Family",
                createdFamily.Id,
                $"Created family {createdFamily.Name}",
                createdFamily.Id,
                null,
                null,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Created($"/family/{createdFamily.Id}",
                    ApiResponse<FamilyDto>.Success(createdFamily, "Family has been successfully created.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateFamilyRequest>>();

        familyGroup.MapPut("/family/{id:guid}", async (Guid userId, Guid id, UpdateFamilyRequest updateFamilyRequest,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var authUserId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (authUserId != userId)
            {
                return Results.Forbid();
            }
            var traceId = httpContext.TraceIdentifier;
            var familyDetail = await familyService.GetFamilyByIdAsync(id, cancellationToken);
            if (familyDetail is null || familyDetail.UserId != authUserId)
            {
                return Results.Forbid();
            }
            updateFamilyRequest.Id = id;

            var updatedFamily = await familyService.UpdateFamilyAsync(updateFamilyRequest, authUserId, cancellationToken);
            await auditService.LogAsync(
                authUserId,
                "Update",
                "Family",
                updatedFamily.Id,
                $"Updated family {updatedFamily.Name}",
                updatedFamily.Id,
                null,
                null,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Ok(ApiResponse<FamilyDto>.Success(updatedFamily, "Family has been successfully updated.", traceId));
        }).AddEndpointFilter<ValidationFilter<UpdateFamilyRequest>>();
    }
}

