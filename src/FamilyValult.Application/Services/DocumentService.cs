using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentReppository;
    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentReppository = documentRepository;
    }
    public Task<DocumentDetailsDto> CreateDocumentsDetailsAsync(CreateDocumentRequest createDocumentRequest)
    {
        
        return Task.FromResult(new DocumentDetailsDto());
    }

    public Task DeleteDocumentsDetailsByIdAsync(Guid documentId)
    {
        
        return Task.FromResult(new DocumentDetailsDto());
    }

    public Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsAsync()
    {
       throw new NotImplementedException();
    }

    public Task<DocumentDetailsDto> GetDocumentsDetailsByIdAsync(Guid documentId)
    {
        return Task.FromResult(new DocumentDetailsDto());
    }

    public Task<DocumentDetailsDto> UpdateDocumentsDetailsAsync(UpdateDocumentRequest updateDocumentRequest)
    {
        return Task.FromResult(new DocumentDetailsDto());
    }
}
