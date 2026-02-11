using FamilyVault.Application.DTOs.Audit;

namespace FamilyVault.Application.Interfaces.Services;

/// <summary>
/// Defines audit logging and retrieval operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs one auditable action.
    /// </summary>
    Task LogAsync(
        Guid userId,
        string action,
        string entityType,
        Guid? entityId,
        string? description,
        Guid? familyId,
        Guid? familyMemberId,
        Guid? documentId,
        string? ipAddress,
        string? metadataJson,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets activity events for a user.
    /// </summary>
    Task<IReadOnlyList<AuditEventDto>> GetActivityAsync(Guid userId, int days, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Gets download history events for a user.
    /// </summary>
    Task<IReadOnlyList<AuditEventDto>> GetDownloadHistoryAsync(Guid userId, int days, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Builds an audit CSV report.
    /// </summary>
    Task<string> BuildCsvReportAsync(Guid userId, int days, CancellationToken cancellationToken);
}
