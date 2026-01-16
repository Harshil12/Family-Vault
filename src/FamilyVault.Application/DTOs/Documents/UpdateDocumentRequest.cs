using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.Documents;

public class UpdateDocumentRequest
{
    public Guid Id { get; set; }

    public DocumentTypes DocumentType { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public DateTimeOffset? IssueDate { get; set; }

    public DateTimeOffset? ExpiryDate { get; set; }

    public Guid FamilyMemberId { get; set; }

}
