namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents the base class for all domain entities.
/// Provides common properties for entity identification,
/// auditing, and soft deletion.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// Stored in UTC.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// Stored in UTC. Null if the entity has never been modified.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity
    /// has been soft-deleted.
    /// When true, the record should be excluded from
    /// standard queries.
    /// </summary>
    public bool IsDeleted { get; set; }
}