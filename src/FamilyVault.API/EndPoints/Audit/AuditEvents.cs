using FamilyVault.Application.DTOs.Audit;
using FamilyVault.Application.Interfaces.Services;
using System.Security.Claims;
using System.Text;

namespace FamilyVault.API.EndPoints.Audit;

/// <summary>
/// Represents AuditEvents.
/// </summary>
public static class AuditEvents
{
    /// <summary>
    /// Performs the MapAuditEndPoints operation.
    /// </summary>
    public static void MapAuditEndPoints(this WebApplication app)
    {
        var auditGroup = app.MapGroup("/audit").RequireAuthorization();

        auditGroup.MapGet("/activity", async (
            int? days,
            int? take,
            IAuditService auditService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;
            var events = await auditService.GetActivityAsync(userId, days ?? 30, take ?? 200, cancellationToken);
            return Results.Ok(ApiResponse<IReadOnlyList<AuditEventDto>>.Success(events, string.Empty, traceId));
        });

        auditGroup.MapGet("/downloads", async (
            int? days,
            int? take,
            IAuditService auditService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;
            var events = await auditService.GetDownloadHistoryAsync(userId, days ?? 30, take ?? 200, cancellationToken);
            return Results.Ok(ApiResponse<IReadOnlyList<AuditEventDto>>.Success(events, string.Empty, traceId));
        });

        auditGroup.MapGet("/export", async (
            int? days,
            IAuditService auditService,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var csv = await auditService.BuildCsvReportAsync(userId, days ?? 30, cancellationToken);
            var fileName = $"audit-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        });
    }
}
