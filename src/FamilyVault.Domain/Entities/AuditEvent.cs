namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents a tracked user action for auditing.
/// </summary>
public class AuditEvent : BaseEntity
{
    /// <summary>
    /// Gets or sets tenant user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets action name (Create, Update, Delete, Download).
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// Gets or sets entity type (Family, FamilyMember, Document, BankAccount).
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// Gets or sets affected entity id.
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Gets or sets related family id.
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// Gets or sets related family member id.
    /// </summary>
    public Guid? FamilyMemberId { get; set; }

    /// <summary>
    /// Gets or sets related document id.
    /// </summary>
    public Guid? DocumentId { get; set; }

    /// <summary>
    /// Gets or sets human readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets requester ip address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets optional metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }
}
