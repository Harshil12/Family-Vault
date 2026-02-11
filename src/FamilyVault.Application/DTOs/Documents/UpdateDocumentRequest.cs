using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.Documents;

/// <summary>
/// Represents UpdateDocumentRequest.
/// </summary>
public class UpdateDocumentRequest
{
    /// <summary>
    /// Gets or sets Id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets DocumentType.
    /// </summary>
    public DocumentTypes DocumentType { get; set; }

    /// <summary>
    /// Gets or sets DocumentNumber.
    /// </summary>
    public string DocumentNumber { get; set; } = null!;

    /// <summary>
    /// Gets or sets IssueDate.
    /// </summary>
    public DateTimeOffset? IssueDate { get; set; }

    /// <summary>
    /// Gets or sets ExpiryDate.
    /// </summary>
    public DateTimeOffset? ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets FamilyMemberId.
    /// </summary>
    public Guid FamilyMemberId { get; set; }

    /// <summary>
    /// Gets or sets SavedLocation.
    /// </summary>
    public string? SavedLocation { get; set; }

}
