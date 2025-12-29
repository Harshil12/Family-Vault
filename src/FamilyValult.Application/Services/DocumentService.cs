using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.Application.Services;

public class DocumentService : IDocumentService
{
    public Task<DocumentDetailsDto> CreateDocumentsDetailsAsync(CreateDocumentRequest createDocumentRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeleteDocumentsDetailsByIdAsync(Guid documentId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<DocumentDetailsDto> GetDocumentsDetailsByIdAsync(Guid documentId)
    {
        throw new NotImplementedException();
    }

    public Task<DocumentDetailsDto> UpdateDocumentsDetailsAsync(UpdateDocumentRequest updateDocumentRequest)
    {
        throw new NotImplementedException();
    }
}
