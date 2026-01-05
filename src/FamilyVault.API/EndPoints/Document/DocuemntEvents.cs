using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FamilyVault.API.EndPoints.Document;

public static class DocuemntEvents
{

    public static void MapDocumentEndPoints(this WebApplication app)
    {
        var documentGroup = app.MapGroup("/documents");

        documentGroup.MapGet("/", async (IDocumentService _documentService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var result = await _documentService.GetDocumentsDetailsAsync();

            if (result is null || !result.Any())
            {
                logger.LogWarning($"No documents found. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<IReadOnlyList<DocumentDetailsDto>>.Failure(
                        message: "No documents found.",
                        errorCode: "DOC_NOT_FOUND",
                        traceId: traceId));
            }
            return Results.Ok(ApiResponse<IReadOnlyList<DocumentDetailsDto>>.Success(result,string.Empty, traceId));
        });

        documentGroup.MapGet("/{id:guid}", async (Guid id, IDocumentService _documentService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var result = await _documentService.GetDocumentDetailsByIdAsync(id);

            if (result is null)
            {
                logger.LogWarning($"No documents found for id : {id}. TraceId: {traceId}");
                return Results.NotFound(ApiResponse<DocumentDetailsDto>.Failure(
                        message: "No documents found for given id",
                        errorCode: "DOC_NOT_FOUND",
                        traceId: traceId));
            }
            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(result, string.Empty, traceId));
        });

        documentGroup.MapDelete("/{id:guid}", async (Guid id, IDocumentService _documentService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            await _documentService.DeleteDocumentDetailsByIdAsync(id);
            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(null, "Dcument has been successfully deleted.", traceId));
        });

        documentGroup.MapPost("/documents", async (CreateDocumentRequest createDocumentRequest, IDocumentService _documentService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var created = await _documentService.CreateDocumentDetailsAsync(createDocumentRequest);
            return Results.Created($"/documents/{created.Id}",
                ApiResponse<DocumentDetailsDto>.Success(created, "Dcument has been successfully created.", traceId));
        });

        documentGroup.MapPut("/documents", async (UpdateDocumentRequest updateDocumentRequest, IDocumentService _documentService, HttpContext httpContext, ILogger logger) =>
        {
            var traceId = httpContext.TraceIdentifier;
            var created = await _documentService.UpdateDocumentDetailsAsync(updateDocumentRequest);
            return Results.Ok(ApiResponse<DocumentDetailsDto>.Success(created, "Dcument has been successfully created.", traceId));
        });
    }
}
