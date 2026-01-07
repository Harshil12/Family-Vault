using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentReppository;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IDocumentRepository documentRepository, IMapper mapper, ILogger<DocumentService> logger)
    {
        _documentReppository = documentRepository;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new document for FamilyMemberId: {FamilyMemberId}", createDocumentRequest.FamilyMemberId);

        var documentToCreate = _mapper.Map<DocumentDetails>(createDocumentRequest);
        documentToCreate.CreatedAt = DateTimeOffset.Now;
        documentToCreate.CreatedBy = "Harshil";

        var result = await _documentReppository.AddAsync(documentToCreate, cancellationToken);

        _logger.LogInformation("Document created with Id: {DocumentId}", result.Id);

        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task DeleteDocumentDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting document with Id: {DocumentId}", documentId);

        await _documentReppository.DeleteByIdAsync(documentId, "Harshil", cancellationToken);

        _logger.LogInformation("Document with Id: {DocumentId} deleted successfully", documentId);
    }

    public async Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsAsync(CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetAllAsync(cancellationToken);

        return _mapper.Map<List<DocumentDetailsDto>>(result);
    }

    public async Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetAsyncbyId(documentId, cancellationToken);

        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task<DocumentDetailsDto> UpdateDocumentDetailsAsync(UpdateDocumentRequest updateDocumentRequest, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating document with Id: {DocumentId}", updateDocumentRequest.Id);

        var documentToUpdate = _mapper.Map<DocumentDetails>(updateDocumentRequest);
        documentToUpdate.UpdatedAt = DateTimeOffset.Now;
        documentToUpdate.UpdatedBy = "Harshil";

        var result = await _documentReppository.UpdateAsync(documentToUpdate, cancellationToken);

        _logger.LogInformation("Document with Id: {DocumentId} updated successfully", result.Id);

        return _mapper.Map<DocumentDetailsDto>(result);
    }
}
