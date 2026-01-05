using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.API.EndPoints.Family;

public static class FamilymemberEvents
{
    public static void MapFamilyEndPoints(this WebApplication app)
    {
        var familyGroup = app.MapGroup("/family");

        familyGroup.MapGet("/", async (IFamilyService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var result = await familyService.GetFamilyAsync();

            if (result is null || !result.Any())
            {
                logger.LogWarning($"No family found. TraceId: {traceId}");
                return Results.Ok(ApiResponse<IReadOnlyList<FamilyDto>>.Success(Array.Empty<FamilyDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<FamilyDto>>.Success(result, string.Empty, traceId));
        });

        familyGroup.MapGet("/{id:Guid}", async (Guid id, IFamilyService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var result = await familyService.GetFamilyByIdAsync(id);

            if (result is null)
            {
                logger.LogWarning($"No family found for id - {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<FamilyDto>.Failure(
                        message: "No family found for given id",
                        errorCode: "FAMILY_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<FamilyDto>.Success(result, string.Empty, traceId));
        });

        familyGroup.MapDelete("/{id:guid}", async (Guid id, IFamilyService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            await familyService.DeleteFamilyByIdAsync(id);

            return Results.Ok(ApiResponse<FamilyDto>.Success(null, "Family has been successfully deleted.", traceId));
        });

        familyGroup.MapPost("/family", async (CreateFamilyRequest createFamilyRequest, IFamilyService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var created = await familyService.CreateFamilyAsync(createFamilyRequest);

            return Results.Created($"/family/{created.Id}",
                    ApiResponse<FamilyDto>.Success(created, "Family has been successfully created.", traceId));
        });

        familyGroup.MapPut("/family/{id:guid}", async (UpdateFamlyRequest updateFamlyRequest, IFamilyService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var updated = await familyService.UpdateFamilyAsync(updateFamlyRequest);

            return Results.Ok(ApiResponse<FamilyDto>.Success(updated, "Family has been successfully updated.", traceId));
        });
    }
}
