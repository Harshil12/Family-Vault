using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.Documents;

public class DocumentDetailsDto
{
    public Guid Id { get; set; }

    public DocumentTypes DocumentType { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public string SavedLocation { get; set; } = null!;

    public DateTimeOffset? IssueDate { get; set; }

    public DateTimeOffset? ExpiryDate { get; set; }

    public Guid FamilyMemberId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string? UpdatedBy { get; set; }
}
