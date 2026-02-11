using FamilyVault.Application.DTOs.BankAccounts;
using FamilyVault.Application.Interfaces.Services;
using System.Security.Claims;

namespace FamilyVault.API.EndPoints.BankAccount;

/// <summary>
/// Represents BankAccountEvents.
/// </summary>
public static class BankAccountEvents
{
    /// <summary>
    /// Performs the MapBankAccountEndPoints operation.
    /// </summary>
    public static void MapBankAccountEndPoints(this WebApplication app)
    {
        var bankAccountGroup = app.MapGroup("/bankaccounts/{familyMemberId:guid}").RequireAuthorization();

        bankAccountGroup.MapGet("/", async (Guid familyMemberId,
            IBankAccountService bankAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("BankAccountEvents");

            var bankAccounts = await bankAccountService.GetBankAccountsByFamilyMemberIdAsync(familyMemberId, cancellationToken);

            if (bankAccounts is null || !bankAccounts.Any())
            {
                logger.LogWarning($"No bank accounts found. TraceId: {traceId}");
                return Results.Ok(ApiResponse<IReadOnlyList<BankAccountDetailsDto>>.Success(Array.Empty<BankAccountDetailsDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<BankAccountDetailsDto>>.Success(bankAccounts, string.Empty, traceId));
        });

        bankAccountGroup.MapGet("/{id:guid}", async (Guid familyMemberId,
            Guid id,
            IBankAccountService bankAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("BankAccountEvents");

            var bankAccount = await bankAccountService.GetBankAccountByIdAsync(id, cancellationToken);

            if (bankAccount is null || bankAccount.FamilyMemberId != familyMemberId)
            {
                logger.LogWarning($"No bank account found for id - {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<BankAccountDetailsDto>.Failure(
                    message: "No bank account found for given id",
                    errorCode: "BANK_ACCOUNT_NOT_FOUND",
                    traceId: traceId));
            }

            return Results.Ok(ApiResponse<BankAccountDetailsDto>.Success(bankAccount, string.Empty, traceId));
        });

        bankAccountGroup.MapPost("/bankaccounts", async (Guid familyMemberId,
            CreateBankAccountRequest createBankAccountRequest,
            IBankAccountService bankAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var traceId = httpContext.TraceIdentifier;
            createBankAccountRequest.FamilyMemberId = familyMemberId;

            var createdBankAccount = await bankAccountService.CreateBankAccountAsync(createBankAccountRequest, userId, cancellationToken);
            await auditService.LogAsync(
                userId,
                "Create",
                "BankAccount",
                createdBankAccount.Id,
                $"Created bank account for {createdBankAccount.BankName}",
                null,
                familyMemberId,
                null,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Created($"/bankaccounts/{createdBankAccount.Id}",
                ApiResponse<BankAccountDetailsDto>.Success(createdBankAccount, "Bank account has been successfully created.", traceId));
        }).AddEndpointFilter<ValidationFilter<CreateBankAccountRequest>>();

        bankAccountGroup.MapPut("/bankaccounts/{id:guid}", async (Guid familyMemberId,
            Guid id,
            UpdateBankAccountRequest updateBankAccountRequest,
            IBankAccountService bankAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var traceId = httpContext.TraceIdentifier;
            updateBankAccountRequest.Id = id;
            updateBankAccountRequest.FamilyMemberId = familyMemberId;

            var updatedBankAccount = await bankAccountService.UpdateBankAccountAsync(updateBankAccountRequest, userId, cancellationToken);
            await auditService.LogAsync(
                userId,
                "Update",
                "BankAccount",
                updatedBankAccount.Id,
                $"Updated bank account for {updatedBankAccount.BankName}",
                null,
                familyMemberId,
                null,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Ok(ApiResponse<BankAccountDetailsDto>.Success(updatedBankAccount, "Bank account has been successfully updated.", traceId));
        }).AddEndpointFilter<ValidationFilter<UpdateBankAccountRequest>>();

        bankAccountGroup.MapDelete("/{id:guid}", async (Guid familyMemberId,
            Guid id,
            IBankAccountService bankAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            ClaimsPrincipal claimsPrincipal,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("BankAccountEvents");

            var existingBankAccount = await bankAccountService.GetBankAccountByIdAsync(id, cancellationToken);
            if (existingBankAccount is null || existingBankAccount.FamilyMemberId != familyMemberId)
            {
                logger.LogWarning($"No bank account found for id - {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<BankAccountDetailsDto>.Failure(
                    message: "No bank account found for given id",
                    errorCode: "BANK_ACCOUNT_NOT_FOUND",
                    traceId: traceId));
            }

            await bankAccountService.DeleteBankAccountByIdAsync(id, userId, cancellationToken);
            await auditService.LogAsync(
                userId,
                "Delete",
                "BankAccount",
                id,
                $"Deleted bank account {id}",
                null,
                familyMemberId,
                null,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Ok(ApiResponse<BankAccountDetailsDto>.Success(null, "Bank account has been successfully deleted.", traceId));
        });
    }

    private static async Task<bool> UserOwnsFamilyMemberAsync(
        Guid familyMemberId,
        Guid userId,
        IFamilyMemberService familyMemberService,
        IFamilyService familyService,
        CancellationToken cancellationToken)
    {
        var familyMember = await familyMemberService.GetFamilyMemberByIdAsync(familyMemberId, cancellationToken);
        if (familyMember is null)
        {
            return false;
        }

        var family = await familyService.GetFamilyByIdAsync(familyMember.FamilyId, cancellationToken);
        return family is not null && family.UserId == userId;
    }
}
