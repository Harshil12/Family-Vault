namespace FamilyVault.Application.DTOs;

/// <summary>
/// Represents BaseDto.
/// </summary>
public class BaseDto
{
    /// <summary>
    /// Gets or sets Id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets CreatedAt.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets CreatedBy.
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// Gets or sets IsDeleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets UpdatedAt.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets UpdatedBy.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
