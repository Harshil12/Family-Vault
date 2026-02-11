using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FamilyVault.API.EndPoints.Document;

/// <summary>
/// Represents DocumentEvents.
/// </summary>
public static class DocumentEvents
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp"
    };

    private static readonly Dictionary<string, string> ContentTypeByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".xls"] = "application/vnd.ms-excel",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".bmp"] = "image/bmp",
        [".webp"] = "image/webp"
    };

    /// <summary>
    /// Performs the MapDocumentEndPoints operation.
    /// </summary>
    public static void MapDocumentEndPoints(this WebApplication app)
    {
        var documentGroup = app.MapGroup("/documents/{familyMemberId:guid}").RequireAuthorization();

        documentGroup.MapGet("/", async (Guid familyMemberId,
            IDocumentService _documentService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("DocumentEvents");

            var documentDetails = await _documentService.GetDocumentsDetailsByFamilyMemberIdAsync(familyMemberId,cancellationToken);

            if (documentDetails is null || !documentDetails.Any())
            {
                logger.LogWarning($"No documents found. TraceId: {traceId}");

                return Results.Ok(ApiResponse<IReadOnlyList<DocumentDetailsDto>>.Success(Array.Empty<DocumentDetailsDto>(), string.Empty, traceId));
            }

            return Results.Ok(ApiResponse<IReadOnlyList<DocumentDetailsDto>>.Success(documentDetails, string.Empty, traceId));

        });

        documentGroup.MapGet("/{id:guid}", async (Guid familyMemberId, Guid id, IDocumentService _documentService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("DocumentEvents");

            var documentDetail = await _documentService.GetDocumentDetailsByIdAsync(id, cancellationToken);

            if (documentDetail is null || documentDetail.FamilyMemberId != familyMemberId)
            {
                logger.LogWarning($"No documents found for id - {id}. TraceId: {traceId}");

                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                        message: "No documents found for given id",
                        errorCode: "DOC_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(documentDetail, string.Empty, traceId));

        });

        documentGroup.MapGet("/{id:guid}/file", async (Guid familyMemberId,
            Guid id,
            bool? download,
            IDocumentService documentService,
            IAuditService auditService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            var traceId = httpContext.TraceIdentifier;
            var document = await documentService.GetDocumentDetailsByIdAsync(id, cancellationToken);

            if (document is null || document.FamilyMemberId != familyMemberId)
            {
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "No documents found for given id",
                    errorCode: "DOC_NOT_FOUND",
                    traceId: traceId));
            }

            if (string.IsNullOrWhiteSpace(document.SavedLocation))
            {
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "No file is attached to this document.",
                    errorCode: "DOC_FILE_NOT_FOUND",
                    traceId: traceId));
            }

            var normalizedRelative = document.SavedLocation.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            var contentRoot = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
            var fullPath = Path.GetFullPath(Path.Combine(contentRoot, normalizedRelative));
            var uploadsRoot = Path.GetFullPath(Path.Combine(contentRoot, "uploads"));

            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            {
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "Document file not found on server.",
                    errorCode: "DOC_FILE_NOT_FOUND",
                    traceId: traceId));
            }

            var extension = Path.GetExtension(fullPath);
            var contentType = ContentTypeByExtension.TryGetValue(extension, out var value) ? value : "application/octet-stream";
            var fileName = Path.GetFileName(fullPath);

            if (download == true)
            {
                await auditService.LogAsync(
                    userId,
                    "Download",
                    "Document",
                    id,
                    $"Downloaded file for document {id}",
                    null,
                    familyMemberId,
                    id,
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    null,
                    cancellationToken);
            }

            return Results.File(
                fileContents: await File.ReadAllBytesAsync(fullPath, cancellationToken),
                contentType: contentType,
                fileDownloadName: download == true ? fileName : null
            );
        });

        documentGroup.MapDelete("/{id:guid}", async (Guid familyMemberId, Guid id, IDocumentService _documentService,
            IAuditService auditService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var traceId = httpContext.TraceIdentifier;
            var existingDocument = await _documentService.GetDocumentDetailsByIdAsync(id, cancellationToken);
            if (existingDocument is null || existingDocument.FamilyMemberId != familyMemberId)
            {
                return Results.Forbid();
            }

            await _documentService.DeleteDocumentDetailsByIdAsync(id, userId, cancellationToken);
            await auditService.LogAsync(
                userId,
                "Delete",
                "Document",
                id,
                $"Deleted document {id}",
                null,
                familyMemberId,
                id,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(null, "Document has been successfully deleted.", traceId));

        });

        documentGroup.MapPost("/documents", async (CreateDocumentRequest createDocumentRequest,
            Guid familyMemberId,
            IDocumentService _documentService,
            IAuditService auditService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var traceId = httpContext.TraceIdentifier;
            createDocumentRequest.FamilyMemberId = familyMemberId;

            var createdDocument = await _documentService.CreateDocumentDetailsAsync(createDocumentRequest, userId, cancellationToken);
            await auditService.LogAsync(
                userId,
                "Create",
                "Document",
                createdDocument.Id,
                $"Created document {createdDocument.DocumentNumber}",
                null,
                familyMemberId,
                createdDocument.Id,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Created($"/documents/{createdDocument.Id}",
                ApiResponse<DocumentDetailsDto>.Success(createdDocument, "Document has been successfully createdDocument.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateDocumentRequest>>();

        documentGroup.MapPost("/documents/upload", async (Guid familyMemberId,
            [FromForm] UploadDocumentRequest uploadDocumentRequest,
            IDocumentService documentService,
            IAuditService auditService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            IWebHostEnvironment hostEnvironment,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }

            if (uploadDocumentRequest.File is null || uploadDocumentRequest.File.Length <= 0)
            {
                return Results.BadRequest(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "File is required.",
                    errorCode: "FILE_REQUIRED",
                    traceId: traceId));
            }
            if (uploadDocumentRequest.File.Length > MaxFileSizeBytes)
            {
                return Results.BadRequest(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "File size cannot exceed 10 MB.",
                    errorCode: "FILE_TOO_LARGE",
                    traceId: traceId));
            }

            var extension = Path.GetExtension(uploadDocumentRequest.File.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedFileExtensions.Contains(extension))
            {
                return Results.BadRequest(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "Only PDF, Word, Excel and image files are allowed.",
                    errorCode: "FILE_TYPE_NOT_ALLOWED",
                    traceId: traceId));
            }

            var familyMember = await familyMemberService.GetFamilyMemberByIdAsync(familyMemberId, cancellationToken);
            if (familyMember is null)
            {
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "No family member found for given id",
                    errorCode: "FAMILY_MEMBER_NOT_FOUND",
                    traceId: traceId));
            }

            var family = await familyService.GetFamilyByIdAsync(familyMember.FamilyId, cancellationToken);
            if (family is null)
            {
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "No family found for family member",
                    errorCode: "FAMILY_NOT_FOUND",
                    traceId: traceId));
            }

            var safeFamilyName = SanitizePathPart(family.Name);
            var safeFileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var relativePath = Path.Combine("uploads", userId.ToString(), safeFamilyName, safeFileName);
            var fullPath = Path.Combine(hostEnvironment.ContentRootPath, relativePath);

            var directoryPath = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(directoryPath);

            await using (var stream = File.Create(fullPath))
            {
                await uploadDocumentRequest.File.CopyToAsync(stream, cancellationToken);
            }

            var createDocumentRequest = new CreateDocumentRequest
            {
                FamilyMemberId = familyMemberId,
                DocumentType = uploadDocumentRequest.DocumentType,
                DocumentNumber = uploadDocumentRequest.DocumentNumber,
                IssueDate = uploadDocumentRequest.IssueDate,
                ExpiryDate = uploadDocumentRequest.ExpiryDate,
                SavedLocation = relativePath.Replace("\\", "/")
            };

            var createdDocument = await documentService.CreateDocumentDetailsAsync(createDocumentRequest, userId, cancellationToken);
            await auditService.LogAsync(
                userId,
                "Upload",
                "Document",
                createdDocument.Id,
                $"Uploaded file for document {createdDocument.DocumentNumber}",
                family.Id,
                familyMemberId,
                createdDocument.Id,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Created($"/documents/{createdDocument.Id}",
                ApiResponse<DocumentDetailsDto>.Success(createdDocument, "Document has been successfully uploaded.", traceId));
        });

        documentGroup.MapPut("/documents/{id:Guid}/file", async (Guid familyMemberId,
            Guid id,
            [FromForm] ReplaceDocumentFileRequest replaceDocumentFileRequest,
            IDocumentService documentService,
            IAuditService auditService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            IWebHostEnvironment hostEnvironment,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;

            if (replaceDocumentFileRequest.File is null || replaceDocumentFileRequest.File.Length <= 0)
            {
                return Results.BadRequest(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "File is required.",
                    errorCode: "FILE_REQUIRED",
                    traceId: traceId));
            }
            if (replaceDocumentFileRequest.File.Length > MaxFileSizeBytes)
            {
                return Results.BadRequest(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "File size cannot exceed 10 MB.",
                    errorCode: "FILE_TOO_LARGE",
                    traceId: traceId));
            }

            var extension = Path.GetExtension(replaceDocumentFileRequest.File.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedFileExtensions.Contains(extension))
            {
                return Results.BadRequest(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "Only PDF, Word, Excel and image files are allowed.",
                    errorCode: "FILE_TYPE_NOT_ALLOWED",
                    traceId: traceId));
            }

            var existingDocument = await documentService.GetDocumentDetailsByIdAsync(id, cancellationToken);
            if (existingDocument is null || existingDocument.FamilyMemberId != familyMemberId)
            {
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "No documents found for given id",
                    errorCode: "DOC_NOT_FOUND",
                    traceId: traceId));
            }

            var familyMember = await familyMemberService.GetFamilyMemberByIdAsync(familyMemberId, cancellationToken);
            if (familyMember is null)
            {
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "No family member found for given id",
                    errorCode: "FAMILY_MEMBER_NOT_FOUND",
                    traceId: traceId));
            }

            var family = await familyService.GetFamilyByIdAsync(familyMember.FamilyId, cancellationToken);
            if (family is null)
            {
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "No family found for family member",
                    errorCode: "FAMILY_NOT_FOUND",
                    traceId: traceId));
            }

            var safeFamilyName = SanitizePathPart(family.Name);
            var safeFileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var relativePath = Path.Combine("uploads", userId.ToString(), safeFamilyName, safeFileName);
            var fullPath = Path.Combine(hostEnvironment.ContentRootPath, relativePath);

            var directoryPath = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(directoryPath);

            await using (var stream = File.Create(fullPath))
            {
                await replaceDocumentFileRequest.File.CopyToAsync(stream, cancellationToken);
            }

            var updateDocumentRequest = new UpdateDocumentRequest
            {
                Id = id,
                FamilyMemberId = familyMemberId,
                DocumentType = replaceDocumentFileRequest.DocumentType ?? existingDocument.DocumentType,
                DocumentNumber = string.IsNullOrWhiteSpace(replaceDocumentFileRequest.DocumentNumber)
                    ? existingDocument.DocumentNumber
                    : replaceDocumentFileRequest.DocumentNumber,
                IssueDate = replaceDocumentFileRequest.IssueDate ?? existingDocument.IssueDate,
                ExpiryDate = replaceDocumentFileRequest.ExpiryDate ?? existingDocument.ExpiryDate,
                SavedLocation = relativePath.Replace("\\", "/")
            };

            var updatedDocument = await documentService.UpdateDocumentDetailsAsync(updateDocumentRequest, userId, cancellationToken);
            TryDeleteOldDocumentFile(existingDocument.SavedLocation, hostEnvironment.ContentRootPath);
            await auditService.LogAsync(
                userId,
                "Update",
                "Document",
                updatedDocument.Id,
                $"Replaced file for document {updatedDocument.DocumentNumber}",
                family.Id,
                familyMemberId,
                updatedDocument.Id,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(updatedDocument, "Document file has been successfully replaced.", traceId));
        });

        documentGroup.MapPut("/documents/{id:Guid}", async (Guid id, UpdateDocumentRequest updateDocumentRequest,
            Guid familyMemberId,
            IDocumentService _documentService,
            IAuditService auditService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;
            if (!await UserOwnsFamilyMemberAsync(familyMemberId, userId, familyMemberService, familyService, cancellationToken))
            {
                return Results.Forbid();
            }
            var existingDocument = await _documentService.GetDocumentDetailsByIdAsync(id, cancellationToken);
            if (existingDocument is null || existingDocument.FamilyMemberId != familyMemberId)
            {
                return Results.Forbid();
            }
            updateDocumentRequest.Id = id;
            updateDocumentRequest.FamilyMemberId = familyMemberId;

            var updatedDocument = await _documentService.UpdateDocumentDetailsAsync(updateDocumentRequest, userId, cancellationToken);
            await auditService.LogAsync(
                userId,
                "Update",
                "Document",
                updatedDocument.Id,
                $"Updated document {updatedDocument.DocumentNumber}",
                null,
                familyMemberId,
                updatedDocument.Id,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                null,
                cancellationToken);

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(updatedDocument, "Document has been successfully updatedDocument.", traceId));

        }).AddEndpointFilter<ValidationFilter<UpdateDocumentRequest>>(); ;
    }

    private static string SanitizePathPart(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return string.IsNullOrWhiteSpace(value) ? "UnknownFamily" : value.Trim();
    }

    private static void TryDeleteOldDocumentFile(string? oldRelativePath, string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(oldRelativePath))
        {
            return;
        }

        var normalized = oldRelativePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(contentRootPath, normalized));
        var uploadsRoot = Path.GetFullPath(Path.Combine(contentRootPath, "uploads"));

        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private static async Task<bool> UserOwnsFamilyMemberAsync(
        Guid familyMemberId,
        Guid userId,
        IFamilyMemberService familyMemberService,
        IFamilyService familyService,
        CancellationToken cancellationToken)
    {
        var familyMember = await familyMemberService.GetFamilyMemberByIdAsync(familyMemberId, cancellationToken);
        if (familyMember is null)
        {
            return false;
        }

        var family = await familyService.GetFamilyByIdAsync(familyMember.FamilyId, cancellationToken);
        return family is not null && family.UserId == userId;
    }
}
