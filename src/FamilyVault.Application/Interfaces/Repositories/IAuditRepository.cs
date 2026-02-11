using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

/// <summary>
/// Defines data access methods for audit events.
/// </summary>
public interface IAuditRepository
{
    /// <summary>
    /// Adds an audit event.
    /// </summary>
    Task<AuditEvent> AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken);

    /// <summary>
    /// Gets audit activity for a user.
    /// </summary>
    Task<IReadOnlyList<AuditEvent>> GetActivityByUserAsync(Guid userId, DateTimeOffset fromUtc, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Gets document download events for a user.
    /// </summary>
    Task<IReadOnlyList<AuditEvent>> GetDownloadHistoryByUserAsync(Guid userId, DateTimeOffset fromUtc, int take, CancellationToken cancellationToken);
}
