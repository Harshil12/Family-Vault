using FamilyVault.Domain.Enums;

namespace FamilyVault.Application.DTOs.Documents;

public class CreateDocumentRequest
{
    /// <summary>
    /// Type of the document (Passport, Aadhar, PAN, DrivingLicense, etc.)
    /// </summary>
    public DocumentTypes DocumentType { get; set; }

    /// <summary>
    /// Unique document identifier or number.
    /// </summary>
    public string DocumentNumber { get; set; } = null!;
   
    /// <summary>
    /// Date when the document was issued.
    /// </summary>
    public DateTimeOffset? IssueDate { get; set; }

    /// <summary>
    /// Expiry date of the document.
    /// </summary>
    public DateTimeOffset? ExpiryDate { get; set; }

    /// <summary>
    /// Family member to whom this document belongs.
    /// </summary>
    public Guid FamilyMemberId { get; set; }
}
