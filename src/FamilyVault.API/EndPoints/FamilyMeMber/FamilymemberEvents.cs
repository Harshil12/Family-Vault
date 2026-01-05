using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.API.EndPoints.FamilyMeMber;

public static class FamilyMemberEvents
{
    public static void MapFamilyMemberEndPoints(this WebApplication app)
    {
        var familyGroup = app.MapGroup("/familymember");

        familyGroup.MapGet("/", async (IFamilymemeberService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var result = await familyService.GetFamilyMembersAsync();

            if (result is null || !result.Any())
            {
                logger.LogWarning($"No family members found. TraceId: {traceId}");
                return Results.Ok(ApiResponse<IReadOnlyList<FamilyMemberDto>>.Success(Array.Empty<FamilyMemberDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<FamilyMemberDto>>.Success(result, string.Empty, traceId));
        });

        familyGroup.MapGet("/{id:Guid}", async (Guid id, IFamilymemeberService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var result = await familyService.GetFamilyMemberByIdAsync(id);

            if (result is null)
            {
                logger.LogWarning($"No family member found for id - {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<FamilyMemberDto>.Failure(
                        message: "No family member found for given id",
                        errorCode: "FAMILY_MEMBER_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(result, string.Empty, traceId));
        });

        familyGroup.MapDelete("/{id:guid}", async (Guid id, IFamilymemeberService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            await familyService.DeleteFamilyMemberByIdAsync(id);

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(null, "Family member has been successfully deleted.", traceId));
        });

        familyGroup.MapPost("/familymember", async (CreateFamilyMememberRequest createFamilyRequest, IFamilymemeberService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var created = await familyService.CreateFamilyMemberAsync(createFamilyRequest);

            return Results.Created($"/familymember/{created.Id}",
                    ApiResponse<FamilyMemberDto>.Success(created, "Family member has been successfully created.", traceId));
        });

        familyGroup.MapPut("/familymember/{id:guid}", async (UpdateFamilyMememberRequest updateFamlyRequest, IFamilymemeberService familyService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var updated = await familyService.UpdateFamilyMemberAsync(updateFamlyRequest);

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(updated, "Family member has been successfully updated.", traceId));
        });
    }
}
