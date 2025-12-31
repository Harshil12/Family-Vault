using FamilyVault.Application.DTOs.Documents;

namespace FamilyVault.Application.Interfaces.Services;

/// <summary>
/// Defines a contract for managing and accessing document details, including retrieval, creation, updating, and
/// deletion operations.
/// </summary>
/// <remarks>All operations are asynchronous and return tasks that complete when the requested action has
/// finished. Implementations should ensure thread safety for concurrent access. Methods that accept request objects
/// require non-null arguments; passing null may result in an exception. Returned document details may be null or empty
/// if the specified document does not exist or if no documents are available.</remarks>
public interface IDocumentService
{
    /// <summary>
    /// Asynchronously retrieves the details of a document specified by its unique identifier.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to retrieve details for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="DocumentDetailsDto"/>
    /// with the details of the specified document, or <see langword="null"/> if the document does not exist.</returns>
    public Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId);

    /// <summary>
    /// Asynchronously retrieves detailed information for all available documents.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of <see
    /// cref="DocumentDetailsDto"/> objects, each representing the details of a document. The list is empty if no
    /// documents are available.</returns>
    public Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsAsync();

    /// <summary>
    /// Asynchronously updates the details of a document based on the specified update request.
    /// </summary>
    /// <param name="updateDocumentRequest">An object containing the updated information for the document. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="DocumentDetailsDto"/>
    /// with the updated document details.</returns>
    public Task<DocumentDetailsDto> UpdateDocumentDetailsAsync(UpdateDocumentRequest updateDocumentRequest);

    /// <summary>
    /// Creates a new document and returns detailed information about the created document.
    /// </summary>
    /// <param name="createDocumentRequest">The request containing the data required to create the document. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="DocumentDetailsDto"/>
    /// with details of the newly created document.</returns>
    public Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest);

    /// <summary>
    /// Asynchronously deletes the details of the document identified by the specified unique identifier.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document whose details are to be deleted.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public Task DeleteDocumentDetailsByIdAsync(Guid documentId);
}
