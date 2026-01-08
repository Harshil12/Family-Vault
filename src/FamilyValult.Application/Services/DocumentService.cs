using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly ICryptoService _cryptoService;
    private readonly IDocumentRepository _documentReppository;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IDocumentRepository documentRepository, ICryptoService cryptoService, IMapper mapper, ILogger<DocumentService> logger)
    {
        _cryptoService = cryptoService;
        _documentReppository = documentRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsAsync(CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetAllAsync(cancellationToken);

        foreach (var doc in result)
        {
            doc.DocumentNumber = _cryptoService.DecryptData(doc.DocumentNumber);
        }

        return _mapper.Map<List<DocumentDetailsDto>>(result);
    }

    public async Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetAsyncbyId(documentId, cancellationToken);
        
        result.DocumentNumber = _cryptoService.DecryptData(result.DocumentNumber);
      
        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new document for FamilyMemberId: {FamilyMemberId}", createDocumentRequest.FamilyMemberId);

        var documentToCreate = _mapper.Map<DocumentDetails>(createDocumentRequest);
        documentToCreate.CreatedAt = DateTimeOffset.Now;
        documentToCreate.CreatedBy = userId.ToString();
        documentToCreate.DocumentNumber = _cryptoService.EncryptData(documentToCreate.DocumentNumber);

        var result = await _documentReppository.AddAsync(documentToCreate, cancellationToken);

        _logger.LogInformation("Document created with Id: {DocumentId}", result.Id);

        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task DeleteDocumentDetailsByIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting document with Id: {DocumentId}", documentId);

        await _documentReppository.DeleteByIdAsync(documentId, userId.ToString(), cancellationToken);

        _logger.LogInformation("Document with Id: {DocumentId} deleted successfully", documentId);
    }


    public async Task<DocumentDetailsDto> UpdateDocumentDetailsAsync(UpdateDocumentRequest updateDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating document with Id: {DocumentId}", updateDocumentRequest.Id);

        var documentToUpdate = _mapper.Map<DocumentDetails>(updateDocumentRequest);
        documentToUpdate.UpdatedAt = DateTimeOffset.Now;
        documentToUpdate.UpdatedBy = userId.ToString();
        documentToUpdate.DocumentNumber = _cryptoService.EncryptData(documentToUpdate.DocumentNumber);

        var result = await _documentReppository.UpdateAsync(documentToUpdate, cancellationToken);

        _logger.LogInformation("Document with Id: {DocumentId} updated successfully", result.Id);

        return _mapper.Map<DocumentDetailsDto>(result);
    }
}
