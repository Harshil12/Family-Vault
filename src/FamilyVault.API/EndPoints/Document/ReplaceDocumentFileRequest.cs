using FamilyVault.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FamilyVault.API.EndPoints.Document;

/// <summary>
/// Represents replace document file request.
/// </summary>
public class ReplaceDocumentFileRequest
{
    /// <summary>
    /// New document file.
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Optional document type override.
    /// </summary>
    public DocumentTypes? DocumentType { get; set; }

    /// <summary>
    /// Optional document number override.
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Optional issue date override.
    /// </summary>
    public DateTimeOffset? IssueDate { get; set; }

    /// <summary>
    /// Optional expiry date override.
    /// </summary>
    public DateTimeOffset? ExpiryDate { get; set; }
}
