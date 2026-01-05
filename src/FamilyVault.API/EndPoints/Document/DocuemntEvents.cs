using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.API.EndPoints.Document;

public static class DocumentEvents
{

    public static void MapDocumentEndPoints(this WebApplication app)
    {
        var documentGroup = app.MapGroup("/documents");

        documentGroup.MapGet("/", async (IDocumentService _documentService, HttpContext httpContext, ILoggerFactory loggerFactory) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("DocumentEvents");

            var result = await _documentService.GetDocumentsDetailsAsync();

            if (result is null || !result.Any())
            {
                logger.LogWarning($"No documents found. TraceId: {traceId}");
                return Results.Ok(ApiResponse<IReadOnlyList<DocumentDetailsDto>>.Success(Array.Empty<DocumentDetailsDto>(), string.Empty, traceId));
            }
            
            return Results.Ok(ApiResponse<IReadOnlyList<DocumentDetailsDto>>.Success(result, string.Empty, traceId));
        });

        documentGroup.MapGet("/{id:guid}", async (Guid id, IDocumentService _documentService, HttpContext httpContext, ILoggerFactory loggerFactory) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("DocumentEvents");

            var result = await _documentService.GetDocumentDetailsByIdAsync(id);

            if (result is null)
            {
                logger.LogWarning($"No documents found for id - {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                        message: "No documents found for given id",
                        errorCode: "DOC_NOT_FOUND",
                        traceId: traceId));
            }

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(result, string.Empty, traceId));
        });

        documentGroup.MapDelete("/{id:guid}", async (Guid id, IDocumentService _documentService, HttpContext httpContext) =>
        {
            var traceId = httpContext.TraceIdentifier;
            await _documentService.DeleteDocumentDetailsByIdAsync(id);

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(null, "Document has been successfully deleted.", traceId));
        });

        documentGroup.MapPost("/documents", async (CreateDocumentRequest createDocumentRequest, IDocumentService _documentService, HttpContext httpContext) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var created = await _documentService.CreateDocumentDetailsAsync(createDocumentRequest);

            return Results.Created($"/documents/{created.Id}",
                ApiResponse<DocumentDetailsDto>.Success(created, "Document has been successfully created.", traceId));
        });

        documentGroup.MapPut("/documents/{id:Guid}", async (Guid id, UpdateDocumentRequest updateDocumentRequest, IDocumentService _documentService, HttpContext httpContext) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var updated = await _documentService.UpdateDocumentDetailsAsync(updateDocumentRequest);

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(updated, "Document has been successfully updated.", traceId));
        });
    }
}
