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
    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp"
    };

    /// <summary>
    /// Performs the MapDocumentEndPoints operation.
    /// </summary>
    public static void MapDocumentEndPoints(this WebApplication app)
    {
        var documentGroup = app.MapGroup("/documents/{familyMemberId:guid}").RequireAuthorization();

        documentGroup.MapGet("/", async (Guid familyMemberId,
            IDocumentService _documentService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
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

        documentGroup.MapGet("/{id:guid}", async (Guid id, IDocumentService _documentService,
            HttpContext httpContext,
            ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("DocumentEvents");

            var documentDetail = await _documentService.GetDocumentDetailsByIdAsync(id, cancellationToken);

            if (documentDetail is null)
            {
                logger.LogWarning($"No documents found for id - {id}. TraceId: {traceId}");

                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                        message: "No documents found for given id",
                        errorCode: "DOC_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(documentDetail, string.Empty, traceId));

        });

        documentGroup.MapDelete("/{id:guid}", async (Guid id, IDocumentService _documentService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;

            await _documentService.DeleteDocumentDetailsByIdAsync(id, userId, cancellationToken);

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(null, "Document has been successfully deleted.", traceId));

        });

        documentGroup.MapPost("/documents", async (CreateDocumentRequest createDocumentRequest,
            IDocumentService _documentService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;

            var createdDocument = await _documentService.CreateDocumentDetailsAsync(createDocumentRequest, userId, cancellationToken);

            return Results.Created($"/documents/{createdDocument.Id}",
                ApiResponse<DocumentDetailsDto>.Success(createdDocument, "Document has been successfully createdDocument.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateDocumentRequest>>();

        documentGroup.MapPost("/documents/upload", async (Guid familyMemberId,
            [FromForm] UploadDocumentRequest uploadDocumentRequest,
            IDocumentService documentService,
            IFamilyMemberService familyMemberService,
            IFamilyService familyService,
            IWebHostEnvironment hostEnvironment,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;

            if (uploadDocumentRequest.File is null || uploadDocumentRequest.File.Length <= 0)
            {
                return Results.BadRequest(ApiResponse<DocumentDetailsDto>.Failure(
                    message: "File is required.",
                    errorCode: "FILE_REQUIRED",
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

            return Results.Created($"/documents/{createdDocument.Id}",
                ApiResponse<DocumentDetailsDto>.Success(createdDocument, "Document has been successfully uploaded.", traceId));
        });

        documentGroup.MapPut("/documents/{id:Guid}", async (Guid id, UpdateDocumentRequest updateDocumentRequest,
            IDocumentService _documentService,
            HttpContext httpContext,
            ClaimsPrincipal claimsPrincipal,
            CancellationToken cancellationToken) =>
        {
            var userId = Helper.GetUserIdFromClaims(claimsPrincipal);
            var traceId = httpContext.TraceIdentifier;

            var updatedDocument = await _documentService.UpdateDocumentDetailsAsync(updateDocumentRequest, userId, cancellationToken);

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
}
