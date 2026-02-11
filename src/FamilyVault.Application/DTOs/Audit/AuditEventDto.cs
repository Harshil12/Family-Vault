namespace FamilyVault.Application.DTOs.Audit;

/// <summary>
/// Represents audit event details returned to clients.
/// </summary>
public class AuditEventDto : BaseDto
{
    /// <summary>
    /// Gets or sets user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets action.
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// Gets or sets entity type.
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// Gets or sets entity id.
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Gets or sets family id.
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Gets or sets family member id.
    /// </summary>
    public Guid? FamilyMemberId { get; set; }

    /// <summary>
    /// Gets or sets document id.
    /// </summary>
    public Guid? DocumentId { get; set; }

    /// <summary>
    /// Gets or sets description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets ip address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets metadata JSON.
    /// </summary>
    public string? MetadataJson { get; set; }
}
