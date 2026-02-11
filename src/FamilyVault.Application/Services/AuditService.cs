using AutoMapper;
using FamilyVault.Application.DTOs.Audit;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Services;

/// <summary>
/// Represents AuditService.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of AuditService.
    /// </summary>
    public AuditService(IAuditRepository auditRepository, IMapper mapper)
    {
        _auditRepository = auditRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Performs LogAsync operation.
    /// </summary>
    public async Task LogAsync(
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
        CancellationToken cancellationToken)
    {
        var auditEvent = new AuditEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            FamilyId = familyId,
            FamilyMemberId = familyMemberId,
            DocumentId = documentId,
            Description = description,
            IpAddress = ipAddress,
            MetadataJson = metadataJson,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            IsDeleted = false
        };

        await _auditRepository.AddAsync(auditEvent, cancellationToken);
    }

    /// <summary>
    /// Performs GetActivityAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<AuditEventDto>> GetActivityAsync(Guid userId, int days, int take, CancellationToken cancellationToken)
    {
        var fromUtc = DateTimeOffset.UtcNow.AddDays(-NormalizeDays(days));
        var events = await _auditRepository.GetActivityByUserAsync(userId, fromUtc, NormalizeTake(take), cancellationToken);
        return _mapper.Map<IReadOnlyList<AuditEventDto>>(events);
    }

    /// <summary>
    /// Performs GetDownloadHistoryAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<AuditEventDto>> GetDownloadHistoryAsync(Guid userId, int days, int take, CancellationToken cancellationToken)
    {
        var fromUtc = DateTimeOffset.UtcNow.AddDays(-NormalizeDays(days));
        var events = await _auditRepository.GetDownloadHistoryByUserAsync(userId, fromUtc, NormalizeTake(take), cancellationToken);
        return _mapper.Map<IReadOnlyList<AuditEventDto>>(events);
    }

    /// <summary>
    /// Performs BuildCsvReportAsync operation.
    /// </summary>
    public async Task<string> BuildCsvReportAsync(Guid userId, int days, CancellationToken cancellationToken)
    {
        var events = await GetActivityAsync(userId, days, 5000, cancellationToken);

        var lines = new List<string>
        {
            "TimestampUtc,Action,EntityType,EntityId,FamilyId,FamilyMemberId,DocumentId,Description,IpAddress,ActorUserId"
        };

        foreach (var item in events)
        {
            lines.Add(string.Join(",",
                Csv(item.CreatedAt.ToUniversalTime().ToString("O")),
                Csv(item.Action),
                Csv(item.EntityType),
                Csv(item.EntityId?.ToString() ?? string.Empty),
                Csv(item.FamilyId?.ToString() ?? string.Empty),
                Csv(item.FamilyMemberId?.ToString() ?? string.Empty),
                Csv(item.DocumentId?.ToString() ?? string.Empty),
                Csv(item.Description ?? string.Empty),
                Csv(item.IpAddress ?? string.Empty),
                Csv(item.UserId.ToString())
            ));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static int NormalizeDays(int days)
    {
        if (days <= 0)
        {
            return 30;
        }
        return Math.Min(days, 365);
    }

    private static int NormalizeTake(int take)
    {
        if (take <= 0)
        {
            return 100;
        }
        return Math.Min(take, 1000);
    }

    private static string Csv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
