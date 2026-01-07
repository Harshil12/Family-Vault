using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

/// <summary>
/// Defines a contract for managing document details in a repository.
/// Provides methods to create, read, update, and delete document records.
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Retrieves the details of a specific document by its unique identifier.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the <see cref="DocumentDetails"/> of the specified document.
    /// </returns>
    public Task<DocumentDetails?> GetAsyncbyId(Guid documentId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the details of all documents in the repository.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains a read-only list of <see cref="DocumentDetails"/>.
    /// </returns>
    public Task<IReadOnlyList<DocumentDetails>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new document record in the repository.
    /// </summary>
    /// <param name="documentDetails">The details of the document to create.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the newly created <see cref="DocumentDetails"/>.
    /// </returns>
    public Task<DocumentDetails> AddAsync(DocumentDetails documentDetails, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the details of an existing document in the repository.
    /// </summary>
    /// <param name="documentDetails">The updated document details.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the updated <see cref="DocumentDetails"/>.
    /// </returns>
    public Task<DocumentDetails> UpdateAsync(DocumentDetails documentDetails, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a document from the repository using its unique identifier.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document to delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public Task DeleteByIdAsync(Guid documentId, string user, CancellationToken cancellationToken);
}
