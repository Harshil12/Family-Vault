using FamilyVault.Application.DTOs.BankAccounts;
using FamilyVault.Application.DTOs.DematAccounts;
using FamilyVault.Application.DTOs.FixedDeposits;
using FamilyVault.Application.DTOs.LifeInsurance;
using FamilyVault.Application.DTOs.Mediclaim;
using FamilyVault.Application.DTOs.MutualFunds;
using FamilyVault.Application.Interfaces.Services;
using System.Security.Claims;

namespace FamilyVault.API.EndPoints.FinancialDetails;

/// <summary>
/// Category-based endpoints for financial details.
/// </summary>
public static class FinancialDetailsEvents
{
    private const string BankAccountsCategory = "bank-accounts";

    /// <summary>
    /// Maps financial details endpoints.
    /// </summary>
    public static void MapFinancialDetailsEndPoints(this WebApplication app)
    {
        var financialGroup = app.MapGroup("/financial-details/{familyMemberId:guid}/{category}").RequireAuthorization();
        var fixedDepositGroup = app.MapGroup("/financial-details/{familyMemberId:guid}/fd").RequireAuthorization();
        var lifeInsuranceGroup = app.MapGroup("/financial-details/{familyMemberId:guid}/life-insurance").RequireAuthorization();
        var mediclaimGroup = app.MapGroup("/financial-details/{familyMemberId:guid}/mediclaim").RequireAuthorization();
        var dematGroup = app.MapGroup("/financial-details/{familyMemberId:guid}/demat-accounts").RequireAuthorization();
        var mutualFundGroup = app.MapGroup("/financial-details/{familyMemberId:guid}/mutual-funds").RequireAuthorization();

        financialGroup.MapGet("/", async (Guid familyMemberId,
            string category,
            IBankAccountService bankAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            if (!IsBankAccountsCategory(category))
            {
                return UnsupportedCategoryResult<IReadOnlyList<BankAccountDetailsDto>>(httpContext.TraceIdentifier);
            }

            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FinancialDetailsEvents");
            var bankAccounts = await bankAccountService.GetBankAccountsByFamilyMemberIdAsync(familyMemberId, cancellationToken);

            if (bankAccounts is null || !bankAccounts.Any())
            {
                logger.LogWarning($"No bank accounts found. TraceId: {traceId}");
                return Results.Ok(ApiResponse<IReadOnlyList<BankAccountDetailsDto>>.Success(Array.Empty<BankAccountDetailsDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<BankAccountDetailsDto>>.Success(bankAccounts, string.Empty, traceId));
        });

        financialGroup.MapGet("/{id:guid}", async (Guid familyMemberId,
            string category,
            Guid id,
            IBankAccountService bankAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            if (!IsBankAccountsCategory(category))
            {
                return UnsupportedCategoryResult<BankAccountDetailsDto>(httpContext.TraceIdentifier);
            }

            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FinancialDetailsEvents");
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

        financialGroup.MapPost("/", async (Guid familyMemberId,
            string category,
            CreateBankAccountRequest createBankAccountRequest,
            IBankAccountService bankAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            if (!IsBankAccountsCategory(category))
            {
                return UnsupportedCategoryResult<BankAccountDetailsDto>(httpContext.TraceIdentifier);
            }

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

            return Results.Created($"/financial-details/{familyMemberId}/{category}/{createdBankAccount.Id}",
                ApiResponse<BankAccountDetailsDto>.Success(createdBankAccount, "Bank account has been successfully created.", traceId));
        }).AddEndpointFilter<ValidationFilter<CreateBankAccountRequest>>();

        financialGroup.MapPut("/{id:guid}", async (Guid familyMemberId,
            string category,
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
            if (!IsBankAccountsCategory(category))
            {
                return UnsupportedCategoryResult<BankAccountDetailsDto>(httpContext.TraceIdentifier);
            }

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

        financialGroup.MapDelete("/{id:guid}", async (Guid familyMemberId,
            string category,
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
            if (!IsBankAccountsCategory(category))
            {
                return UnsupportedCategoryResult<BankAccountDetailsDto>(httpContext.TraceIdentifier);
            }

            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("FinancialDetailsEvents");
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

        fixedDepositGroup.MapGet("/", async (Guid familyMemberId,
            IFixedDepositService fixedDepositService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var list = await fixedDepositService.GetByFamilyMemberIdAsync(familyMemberId, cancellationToken);
            return Results.Ok(ApiResponse<IReadOnlyList<FixedDepositDetailsDto>>.Success(list, string.Empty, httpContext.TraceIdentifier));
        });

        fixedDepositGroup.MapGet("/{id:guid}", async (Guid familyMemberId, Guid id,
            IFixedDepositService fixedDepositService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var item = await fixedDepositService.GetByIdAsync(id, cancellationToken);
            if (item is null || item.FamilyMemberId != familyMemberId)
            {
                return Results.NotFound(ApiResponse<FixedDepositDetailsDto>.Failure("No fixed deposit found for given id", "FD_NOT_FOUND", httpContext.TraceIdentifier));
            }
            return Results.Ok(ApiResponse<FixedDepositDetailsDto>.Success(item, string.Empty, httpContext.TraceIdentifier));
        });

        fixedDepositGroup.MapPost("/", async (Guid familyMemberId, CreateFixedDepositRequest request,
            IFixedDepositService fixedDepositService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            request.FamilyMemberId = familyMemberId;
            var created = await fixedDepositService.CreateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Create", "FixedDeposit", created.Id, $"Created fixed deposit for {created.InstitutionName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Created($"/financial-details/{familyMemberId}/fd/{created.Id}", ApiResponse<FixedDepositDetailsDto>.Success(created, "Fixed deposit has been successfully created.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<CreateFixedDepositRequest>>();

        fixedDepositGroup.MapPut("/{id:guid}", async (Guid familyMemberId, Guid id, UpdateFixedDepositRequest request,
            IFixedDepositService fixedDepositService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            request.Id = id;
            request.FamilyMemberId = familyMemberId;
            var updated = await fixedDepositService.UpdateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Update", "FixedDeposit", updated.Id, $"Updated fixed deposit for {updated.InstitutionName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<FixedDepositDetailsDto>.Success(updated, "Fixed deposit has been successfully updated.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<UpdateFixedDepositRequest>>();

        fixedDepositGroup.MapDelete("/{id:guid}", async (Guid familyMemberId, Guid id,
            IFixedDepositService fixedDepositService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            await fixedDepositService.DeleteAsync(id, userId, cancellationToken);
            await auditService.LogAsync(userId, "Delete", "FixedDeposit", id, $"Deleted fixed deposit {id}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<FixedDepositDetailsDto>.Success(null, "Fixed deposit has been successfully deleted.", httpContext.TraceIdentifier));
        });

        lifeInsuranceGroup.MapGet("/", async (Guid familyMemberId,
            ILifeInsuranceService lifeInsuranceService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var list = await lifeInsuranceService.GetByFamilyMemberIdAsync(familyMemberId, cancellationToken);
            return Results.Ok(ApiResponse<IReadOnlyList<LifeInsurancePolicyDetailsDto>>.Success(list, string.Empty, httpContext.TraceIdentifier));
        });

        lifeInsuranceGroup.MapGet("/{id:guid}", async (Guid familyMemberId, Guid id,
            ILifeInsuranceService lifeInsuranceService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var item = await lifeInsuranceService.GetByIdAsync(id, cancellationToken);
            if (item is null || item.FamilyMemberId != familyMemberId)
            {
                return Results.NotFound(ApiResponse<LifeInsurancePolicyDetailsDto>.Failure("No life insurance policy found for given id", "LIFE_INSURANCE_NOT_FOUND", httpContext.TraceIdentifier));
            }
            return Results.Ok(ApiResponse<LifeInsurancePolicyDetailsDto>.Success(item, string.Empty, httpContext.TraceIdentifier));
        });

        lifeInsuranceGroup.MapPost("/", async (Guid familyMemberId, CreateLifeInsurancePolicyRequest request,
            ILifeInsuranceService lifeInsuranceService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            request.FamilyMemberId = familyMemberId;
            var created = await lifeInsuranceService.CreateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Create", "LifeInsurance", created.Id, $"Created life insurance policy for {created.InsurerName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Created($"/financial-details/{familyMemberId}/life-insurance/{created.Id}", ApiResponse<LifeInsurancePolicyDetailsDto>.Success(created, "Life insurance policy has been successfully created.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<CreateLifeInsurancePolicyRequest>>();

        lifeInsuranceGroup.MapPut("/{id:guid}", async (Guid familyMemberId, Guid id, UpdateLifeInsurancePolicyRequest request,
            ILifeInsuranceService lifeInsuranceService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            request.Id = id;
            request.FamilyMemberId = familyMemberId;
            var updated = await lifeInsuranceService.UpdateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Update", "LifeInsurance", updated.Id, $"Updated life insurance policy for {updated.InsurerName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<LifeInsurancePolicyDetailsDto>.Success(updated, "Life insurance policy has been successfully updated.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<UpdateLifeInsurancePolicyRequest>>();

        lifeInsuranceGroup.MapDelete("/{id:guid}", async (Guid familyMemberId, Guid id,
            ILifeInsuranceService lifeInsuranceService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            await lifeInsuranceService.DeleteAsync(id, userId, cancellationToken);
            await auditService.LogAsync(userId, "Delete", "LifeInsurance", id, $"Deleted life insurance policy {id}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<LifeInsurancePolicyDetailsDto>.Success(null, "Life insurance policy has been successfully deleted.", httpContext.TraceIdentifier));
        });

        mediclaimGroup.MapGet("/", async (Guid familyMemberId,
            IMediclaimService mediclaimService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var list = await mediclaimService.GetByFamilyMemberIdAsync(familyMemberId, cancellationToken);
            return Results.Ok(ApiResponse<IReadOnlyList<MediclaimPolicyDetailsDto>>.Success(list, string.Empty, httpContext.TraceIdentifier));
        });

        mediclaimGroup.MapGet("/{id:guid}", async (Guid familyMemberId, Guid id,
            IMediclaimService mediclaimService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var item = await mediclaimService.GetByIdAsync(id, cancellationToken);
            if (item is null || item.FamilyMemberId != familyMemberId)
            {
                return Results.NotFound(ApiResponse<MediclaimPolicyDetailsDto>.Failure("No mediclaim policy found for given id", "MEDICLAIM_NOT_FOUND", httpContext.TraceIdentifier));
            }
            return Results.Ok(ApiResponse<MediclaimPolicyDetailsDto>.Success(item, string.Empty, httpContext.TraceIdentifier));
        });

        mediclaimGroup.MapPost("/", async (Guid familyMemberId, CreateMediclaimPolicyRequest request,
            IMediclaimService mediclaimService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            request.FamilyMemberId = familyMemberId;
            var created = await mediclaimService.CreateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Create", "Mediclaim", created.Id, $"Created mediclaim policy for {created.InsurerName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Created($"/financial-details/{familyMemberId}/mediclaim/{created.Id}", ApiResponse<MediclaimPolicyDetailsDto>.Success(created, "Mediclaim policy has been successfully created.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<CreateMediclaimPolicyRequest>>();

        mediclaimGroup.MapPut("/{id:guid}", async (Guid familyMemberId, Guid id, UpdateMediclaimPolicyRequest request,
            IMediclaimService mediclaimService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            request.Id = id;
            request.FamilyMemberId = familyMemberId;
            var updated = await mediclaimService.UpdateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Update", "Mediclaim", updated.Id, $"Updated mediclaim policy for {updated.InsurerName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<MediclaimPolicyDetailsDto>.Success(updated, "Mediclaim policy has been successfully updated.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<UpdateMediclaimPolicyRequest>>();

        mediclaimGroup.MapDelete("/{id:guid}", async (Guid familyMemberId, Guid id,
            IMediclaimService mediclaimService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            await mediclaimService.DeleteAsync(id, userId, cancellationToken);
            await auditService.LogAsync(userId, "Delete", "Mediclaim", id, $"Deleted mediclaim policy {id}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<MediclaimPolicyDetailsDto>.Success(null, "Mediclaim policy has been successfully deleted.", httpContext.TraceIdentifier));
        });

        dematGroup.MapGet("/", async (Guid familyMemberId,
            IDematAccountService dematAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var list = await dematAccountService.GetByFamilyMemberIdAsync(familyMemberId, cancellationToken);
            return Results.Ok(ApiResponse<IReadOnlyList<DematAccountDetailsDto>>.Success(list, string.Empty, httpContext.TraceIdentifier));
        });

        dematGroup.MapGet("/{id:guid}", async (Guid familyMemberId, Guid id,
            IDematAccountService dematAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var item = await dematAccountService.GetByIdAsync(id, cancellationToken);
            if (item is null || item.FamilyMemberId != familyMemberId)
            {
                return Results.NotFound(ApiResponse<DematAccountDetailsDto>.Failure("No demat account found for given id", "DEMAT_NOT_FOUND", httpContext.TraceIdentifier));
            }
            return Results.Ok(ApiResponse<DematAccountDetailsDto>.Success(item, string.Empty, httpContext.TraceIdentifier));
        });

        dematGroup.MapPost("/", async (Guid familyMemberId, CreateDematAccountRequest request,
            IDematAccountService dematAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            request.FamilyMemberId = familyMemberId;
            var created = await dematAccountService.CreateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Create", "DematAccount", created.Id, $"Created demat account for {created.BrokerName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Created($"/financial-details/{familyMemberId}/demat-accounts/{created.Id}", ApiResponse<DematAccountDetailsDto>.Success(created, "Demat account has been successfully created.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<CreateDematAccountRequest>>();

        dematGroup.MapPut("/{id:guid}", async (Guid familyMemberId, Guid id, UpdateDematAccountRequest request,
            IDematAccountService dematAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            request.Id = id;
            request.FamilyMemberId = familyMemberId;
            var updated = await dematAccountService.UpdateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Update", "DematAccount", updated.Id, $"Updated demat account for {updated.BrokerName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<DematAccountDetailsDto>.Success(updated, "Demat account has been successfully updated.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<UpdateDematAccountRequest>>();

        dematGroup.MapDelete("/{id:guid}", async (Guid familyMemberId, Guid id,
            IDematAccountService dematAccountService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            await dematAccountService.DeleteAsync(id, userId, cancellationToken);
            await auditService.LogAsync(userId, "Delete", "DematAccount", id, $"Deleted demat account {id}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<DematAccountDetailsDto>.Success(null, "Demat account has been successfully deleted.", httpContext.TraceIdentifier));
        });

        mutualFundGroup.MapGet("/", async (Guid familyMemberId,
            IMutualFundService mutualFundService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var list = await mutualFundService.GetByFamilyMemberIdAsync(familyMemberId, cancellationToken);
            return Results.Ok(ApiResponse<IReadOnlyList<MutualFundHoldingDetailsDto>>.Success(list, string.Empty, httpContext.TraceIdentifier));
        });

        mutualFundGroup.MapGet("/{id:guid}", async (Guid familyMemberId, Guid id,
            IMutualFundService mutualFundService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var item = await mutualFundService.GetByIdAsync(id, cancellationToken);
            if (item is null || item.FamilyMemberId != familyMemberId)
            {
                return Results.NotFound(ApiResponse<MutualFundHoldingDetailsDto>.Failure("No mutual fund holding found for given id", "MUTUAL_FUND_NOT_FOUND", httpContext.TraceIdentifier));
            }
            return Results.Ok(ApiResponse<MutualFundHoldingDetailsDto>.Success(item, string.Empty, httpContext.TraceIdentifier));
        });

        mutualFundGroup.MapPost("/", async (Guid familyMemberId, CreateMutualFundHoldingRequest request,
            IMutualFundService mutualFundService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            request.FamilyMemberId = familyMemberId;
            var created = await mutualFundService.CreateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Create", "MutualFund", created.Id, $"Created mutual fund holding for {created.AMCName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Created($"/financial-details/{familyMemberId}/mutual-funds/{created.Id}", ApiResponse<MutualFundHoldingDetailsDto>.Success(created, "Mutual fund holding has been successfully created.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<CreateMutualFundHoldingRequest>>();

        mutualFundGroup.MapPut("/{id:guid}", async (Guid familyMemberId, Guid id, UpdateMutualFundHoldingRequest request,
            IMutualFundService mutualFundService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            request.Id = id;
            request.FamilyMemberId = familyMemberId;
            var updated = await mutualFundService.UpdateAsync(request, userId, cancellationToken);
            await auditService.LogAsync(userId, "Update", "MutualFund", updated.Id, $"Updated mutual fund holding for {updated.AMCName}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<MutualFundHoldingDetailsDto>.Success(updated, "Mutual fund holding has been successfully updated.", httpContext.TraceIdentifier));
        }).AddEndpointFilter<ValidationFilter<UpdateMutualFundHoldingRequest>>();

        mutualFundGroup.MapDelete("/{id:guid}", async (Guid familyMemberId, Guid id,
            IMutualFundService mutualFundService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            ClaimsPrincipal claimsPrincipal,
            HttpContext httpContext,
            IAuditService auditService,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            await mutualFundService.DeleteAsync(id, userId, cancellationToken);
            await auditService.LogAsync(userId, "Delete", "MutualFund", id, $"Deleted mutual fund holding {id}", null, familyMemberId, null, httpContext.Connection.RemoteIpAddress?.ToString(), null, cancellationToken);
            return Results.Ok(ApiResponse<MutualFundHoldingDetailsDto>.Success(null, "Mutual fund holding has been successfully deleted.", httpContext.TraceIdentifier));
        });
    }

    private static IResult UnsupportedCategoryResult<T>(string traceId)
    {
        return Results.BadRequest(ApiResponse<T>.Failure(
            message: "Unsupported financial detail category.",
            errorCode: "UNSUPPORTED_FINANCIAL_DETAIL_CATEGORY",
            traceId: traceId));
    }

    private static bool IsBankAccountsCategory(string category)
    {
        return string.Equals(category, BankAccountsCategory, StringComparison.OrdinalIgnoreCase)
            || string.Equals(category, "bankaccounts", StringComparison.OrdinalIgnoreCase)
            || string.Equals(category, "bank-accounts", StringComparison.OrdinalIgnoreCase);
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
