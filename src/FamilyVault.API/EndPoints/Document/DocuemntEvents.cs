using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Services;
using System.Threading;

namespace FamilyVault.API.EndPoints.Document;

public static class DocumentEvents
{

    public static void MapDocumentEndPoints(this WebApplication app)
    {
        var documentGroup = app.MapGroup("/documents");

        documentGroup.MapGet("/", async (IDocumentService _documentService, 
            HttpContext httpContext, 
            ILoggerFactory loggerFactory, 
            CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var logger = loggerFactory.CreateLogger("DocumentEvents");

            var documentDetails = await _documentService.GetDocumentsDetailsAsync(cancellationToken);

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
            CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
        
            await _documentService.DeleteDocumentDetailsByIdAsync(id, cancellationToken);

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(null, "Document has been successfully deleted.", traceId));

        });

        documentGroup.MapPost("/documents", async (CreateDocumentRequest createDocumentRequest, 
            IDocumentService _documentService, 
            HttpContext httpContext, 
            CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
     
            var createdDocument = await _documentService.CreateDocumentDetailsAsync(createDocumentRequest, cancellationToken);

            return Results.Created($"/documents/{createdDocument.Id}",
                ApiResponse<DocumentDetailsDto>.Success(createdDocument, "Document has been successfully createdDocument.", traceId));

        }).AddEndpointFilter<ValidationFilter<CreateDocumentRequest>>(); 

        documentGroup.MapPut("/documents/{id:Guid}", async (Guid id, UpdateDocumentRequest updateDocumentRequest, IDocumentService _documentService, HttpContext httpContext, CancellationToken cancellationToken) =>
        {
            var traceId = httpContext.TraceIdentifier;
     
            var updatedDocument = await _documentService.UpdateDocumentDetailsAsync(updateDocumentRequest, cancellationToken);

            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(updatedDocument, "Document has been successfully updatedDocument.", traceId));

        }).AddEndpointFilter<ValidationFilter<UpdateDocumentRequest>>(); ;
    }
}
