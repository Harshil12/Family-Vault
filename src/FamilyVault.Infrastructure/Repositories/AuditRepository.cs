using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

/// <summary>
/// Represents AuditRepository.
/// </summary>
public class AuditRepository : IAuditRepository
{
    private readonly AppDbContext _appDbContext;

    /// <summary>
    /// Initializes a new instance of AuditRepository.
    /// </summary>
    public AuditRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    /// <summary>
    /// Performs AddAsync operation.
    /// </summary>
    public async Task<AuditEvent> AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        await _appDbContext.AddAsync(auditEvent, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return auditEvent;
    }

    /// <summary>
    /// Performs GetActivityByUserAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<AuditEvent>> GetActivityByUserAsync(Guid userId, DateTimeOffset fromUtc, int take, CancellationToken cancellationToken)
    {
        return await _appDbContext.Set<AuditEvent>()
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.CreatedAt >= fromUtc && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Performs GetDownloadHistoryByUserAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<AuditEvent>> GetDownloadHistoryByUserAsync(Guid userId, DateTimeOffset fromUtc, int take, CancellationToken cancellationToken)
    {
        return await _appDbContext.Set<AuditEvent>()
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.CreatedAt >= fromUtc && !a.IsDeleted && a.Action == "Download" && a.EntityType == "Document")
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
