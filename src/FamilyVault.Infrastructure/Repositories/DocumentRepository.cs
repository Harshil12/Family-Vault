using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Infrastructure.Repositories;

internal class DocumentRepository : IdocumentRepository
{
    public Task<DocumentDetails> CreateDocumentsDetailsAsync(DocumentDetails documentDetails)
    {
        throw new NotImplementedException();
    }

    public Task DeleteDocumentsDetailsByIdAsync(Guid documentId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<DocumentDetails>> GetDocumentsDetailsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<DocumentDetails> GetDocumentsDetailsByIdAsync(Guid documentId)
    {
        throw new NotImplementedException();
    }

    public Task<DocumentDetails> UpdateDocumentsDetailsAsync(DocumentDetails documentDetails)
    {
        throw new NotImplementedException();
    }
}
