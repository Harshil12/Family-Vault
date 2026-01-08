using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.Interfaces.Services;
using System.Security.Claims;

namespace FamilyVault.API.EndPoints.FamilyMeMber;

public static class FamilyMemberEvents
{
    public static void MapFamilyMemberEndPoints(this WebApplication app)
    {
        var familyGroup = app.MapGroup("/familymember").RequireAuthorization();

        familyGroup.MapGet("/", async (IFamilymemeberService familyService, HttpContext httpContext, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
            
            var logger = loggerFactory.CreateLogger("FamilyMemberEvents");

            var familyMembers = await familyService.GetFamilyMembersAsync(cancellationToken);

            if (familyMembers is null || !familyMembers.Any())
            {
                logger.LogWarning($"No family members found. TraceId: {traceId}");
             
                return Results.Ok(ApiResponse<IReadOnlyList<FamilyMemberDto>>.Success(Array.Empty<FamilyMemberDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<FamilyMemberDto>>.Success(familyMembers, string.Empty, traceId));

        });

        familyGroup.MapGet("/{id:Guid}", async (Guid id, IFamilymemeberService familyService, HttpContext httpContext, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FamilyMemberEvents");

            var familyMember = await familyService.GetFamilyMemberByIdAsync(id, cancellationToken);

            if (familyMember is null)
            {
                logger.LogWarning($"No family member found for id - {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<FamilyMemberDto>.Failure(
                        message: "No family member found for given id",
                        errorCode: "FAMILY_MEMBER_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(familyMember, string.Empty, traceId));

        });

        familyGroup.MapDelete("/{id:guid}", async (Guid id,
            IFamilymemeberService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;
      
            await familyService.DeleteFamilyMemberByIdAsync(id, userId, cancellationToken);

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(null, "Family member has been successfully deleted.", traceId));

        });

        familyGroup.MapPost("/familymember", async (CreateFamilyMememberRequest createFamilyRequest,
            IFamilymemeberService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;
       
            var createdFamilyMember = await familyService.CreateFamilyMemberAsync(createFamilyRequest, userId, cancellationToken);

            return Results.Created($"/familymember/{createdFamilyMember.Id}",
                    ApiResponse<FamilyMemberDto>.Success(createdFamilyMember, "Family member has been successfully createdFamilyMember.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateFamilyMememberRequest>>();

        familyGroup.MapPut("/familymember/{id:guid}", async (UpdateFamilyMememberRequest updateFamlyRequest,
            IFamilymemeberService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal, 
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;
        
            var updatedFamilyMember = await familyService.UpdateFamilyMemberAsync(updateFamlyRequest, userId, cancellationToken);

            return Results.Ok(ApiResponse<FamilyMemberDto>.Success(updatedFamilyMember, "Family member has been successfully updatedFamilyMember.", traceId));

        }).AddEndpointFilter<ValidationFilter<UpdateFamilyMememberRequest>>();
    }
}
