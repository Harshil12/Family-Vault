using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.API.EndPoints.Family;

public static class FamilymemberEvents
{
    public static void MapFamilyEndPoints(this WebApplication app)
    {
        var familyGroup = app.MapGroup("/family").RequireAuthorization(); 

        familyGroup.MapGet("/", async (IFamilyService familyService, HttpContext httpContext, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FamilyEndpoints");

            var familyDetails = await familyService.GetFamilyAsync(cancellationToken);

            if (familyDetails is null || !familyDetails.Any())
            {
                logger.LogWarning($"No family found. TraceId: {traceId}");

                return Results.Ok(ApiResponse<IReadOnlyList<FamilyDto>>.Success(Array.Empty<FamilyDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<FamilyDto>>.Success(familyDetails, string.Empty, traceId));

        });

        familyGroup.MapGet("/{id:Guid}", async (Guid id, IFamilyService familyService, HttpContext httpContext, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FamilyEndpoints");

            var familyDetail = await familyService.GetFamilyByIdAsync(id, cancellationToken);

            if (familyDetail is null)
            {
                logger.LogWarning($"No family found for id - {id}. TraceId: {traceId}");

                return Results.NotFound(ApiResponse<FamilyDto>.Failure(
                        message: "No family found for given id",
                        errorCode: "FAMILY_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<FamilyDto>.Success(familyDetail, string.Empty, traceId));

        });

        familyGroup.MapDelete("/{id:guid}", async (Guid id, IFamilyService familyService, HttpContext httpContext, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;

            await familyService.DeleteFamilyByIdAsync(id, cancellationToken);

            return Results.Ok(ApiResponse<FamilyDto>.Success(null, "Family has been successfully deleted.", traceId));

        });

        familyGroup.MapPost("/family", async (CreateFamilyRequest createFamilyRequest, IFamilyService familyService, HttpContext httpContext, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;

            var createdFamily = await familyService.CreateFamilyAsync(createFamilyRequest, cancellationToken);

            return Results.Created($"/family/{createdFamily.Id}",
                    ApiResponse<FamilyDto>.Success(createdFamily, "Family has been successfully createdFamily.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateFamilyRequest>>();

        familyGroup.MapPut("/family/{id:guid}", async (UpdateFamlyRequest updateFamlyRequest, IFamilyService familyService, HttpContext httpContext, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;

            var updatedFamily = await familyService.UpdateFamilyAsync(updateFamlyRequest, cancellationToken);

            return Results.Ok(ApiResponse<FamilyDto>.Success(updatedFamily, "Family has been successfully updatedFamily.", traceId));
        }).AddEndpointFilter<ValidationFilter<UpdateFamlyRequest>>();
    }
}
