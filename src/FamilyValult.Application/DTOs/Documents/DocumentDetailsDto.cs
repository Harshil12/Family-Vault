using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.Documents;

public class DocumentDetailsDto : BaseDto
{
    public Guid Id { get; set; }

    public DocumentTypes DocumentType { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public string SavedLocation { get; set; } = null!;

    public DateTimeOffset? IssueDate { get; set; }

    public DateTimeOffset? ExpiryDate { get; set; }

    public Guid FamilyMemberId { get; set; }
}
