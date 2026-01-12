using System.ComponentModel.DataAnnotations;

namespace FamilyVault.Domain.Entities;

/// <summary>
/// Represents a family grouping within the application.
/// A Family is owned by a single <see cref="User"/> and contains metadata such as a display name.
/// Inherits common persistence properties from <see cref="BaseEntity"/>.
/// </summary>
public class Family : BaseEntity
{
    /// <summary>
    /// Human-readable name for the family (e.g. "Smith Family").
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Identifier of the owning <see cref="User"/>.
    /// This acts as the foreign key linking the family to its owner.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the owning <see cref="User"/>.
    /// Should be populated when the owner information is required.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// List of related <see cref="FamilyMember"/> entities that belong to this family.
    /// </summary>
    public ICollection<FamilyMember>? FamilyMembers { get; set; }
}
