using FamilyVault.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FamilyVault.API.EndPoints.Document;

/// <summary>
/// Represents upload document request.
/// </summary>
public class UploadDocumentRequest
{
    /// <summary>
    /// Document file.
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Document type.
    /// </summary>
    public DocumentTypes DocumentType { get; set; }

    /// <summary>
    /// Document number.
    /// </summary>
    public string DocumentNumber { get; set; } = null!;

    /// <summary>
    /// Issue date.
    /// </summary>
    public DateTimeOffset? IssueDate { get; set; }

    /// <summary>
    /// Expiry date.
    /// </summary>
    public DateTimeOffset? ExpiryDate { get; set; }
}
