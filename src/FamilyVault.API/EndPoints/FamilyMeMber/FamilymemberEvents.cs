using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Services;
using System.Security.Claims;

namespace FamilyVault.API.EndPoints.FamilyMeMber;

/// <summary>
/// Represents FamilyMemberEvents.
/// </summary>
public static class FamilyMemberEvents
{
    /// <summary>
    /// Performs the MapFamilyMemberEndPoints operation.
    /// </summary>
    public static void MapFamilyMemberEndPoints(this WebApplication app)
    {
        var familyGroup = app.MapGroup("/familymember/{familyId:guid}").RequireAuthorization();

        familyGroup.MapGet("/", async (Guid familyId, IFamilyMemberService familyMemberService, IFamilyService familyService, HttpContext httpContext, ILoggerFactory loggerFactory, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var family = await familyService.GetFamilyByIdAsync(familyId, cancellationToken);
            if (family is null || family.UserId != userId)
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;

            var logger = loggerFactory.CreateLogger("FamilyMemberEvents");

            var familyMembers = await familyMemberService.GetFamilyMembersByFamilyIdAsync(familyId, cancellationToken);

            if (familyMembers is null || !familyMembers.Any())
            {
                logger.LogWarning($"No family members found. TraceId: {traceId}");

                return Results.Ok(ApiResponse<IReadOnlyList<FamilyMemberDto>>.Success(Array.Empty<FamilyMemberDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<FamilyMemberDto>>.Success(familyMembers, string.Empty, traceId));

        });

        familyGroup.MapGet("/{id:Guid}", async (Guid familyId, Guid id, IFamilyMemberService familyMemberService, IFamilyService familyService, HttpContext httpContext, ILoggerFactory loggerFactory, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var family = await familyService.GetFamilyByIdAsync(familyId, cancellationToken);
            if (family is null || family.UserId != userId)
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FamilyMemberEvents");

            var familyMember = await familyMemberService.GetFamilyMemberByIdAsync(id, cancellationToken);

            if (familyMember is null || familyMember.FamilyId != familyId)
            {
                logger.LogWarning($"No family member found for id - {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<FamilyMemberDto>.Failure(
                        message: "No family member found for given id",
                        errorCode: "FAMILY_MEMBER_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(familyMember, string.Empty, traceId));

        });

        familyGroup.MapDelete("/{id:guid}", async (Guid familyId, Guid id,
            IFamilyMemberService familyService,
            IFamilyService familyOwnerService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var family = await familyOwnerService.GetFamilyByIdAsync(familyId, cancellationToken);
            if (family is null || family.UserId != userId)
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var familyMember = await familyService.GetFamilyMemberByIdAsync(id, cancellationToken);
            if (familyMember is null || familyMember.FamilyId != familyId)
            {
                return Results.Forbid();
            }

            await familyService.DeleteFamilyMemberByIdAsync(id, userId, cancellationToken);

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(null, "Family member has been successfully deleted.", traceId));

        });

        familyGroup.MapPost("/familymember", async (Guid familyId, CreateFamilyMemberRequest createFamilyRequest,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var family = await familyService.GetFamilyByIdAsync(familyId, cancellationToken);
            if (family is null || family.UserId != userId)
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            createFamilyRequest.FamilyId = familyId;

            var createdFamilyMember = await familyMemberService.CreateFamilyMemberAsync(createFamilyRequest, userId, cancellationToken);

            return Results.Created($"/familymember/{createdFamilyMember.Id}",
                    ApiResponse<FamilyMemberDto>.Success(createdFamilyMember, "Family member has been successfully created.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateFamilyMemberRequest>>();

        familyGroup.MapPut("/familymember/{id:guid}", async (UpdateFamilyMemberRequest updateFamilyMemberRequest,
            Guid familyId,
            Guid id,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var family = await familyService.GetFamilyByIdAsync(familyId, cancellationToken);
            if (family is null || family.UserId != userId)
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var existingMember = await familyMemberService.GetFamilyMemberByIdAsync(id, cancellationToken);
            if (existingMember is null || existingMember.FamilyId != familyId)
            {
                return Results.Forbid();
            }
            updateFamilyMemberRequest.Id = id;
            updateFamilyMemberRequest.FamilyId = familyId;

            var updatedFamilyMember = await familyMemberService.UpdateFamilyMemberAsync(updateFamilyMemberRequest, userId, cancellationToken);

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(updatedFamilyMember, "Family member has been successfully updated.", traceId));

        }).AddEndpointFilter<ValidationFilter<UpdateFamilyMemberRequest>>();
    }
}

