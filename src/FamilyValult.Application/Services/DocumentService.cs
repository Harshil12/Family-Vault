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
    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest)
    {
        _logger.LogInformation("Creating a new document for FamilyMemberId: {FamilyMemberId}", createDocumentRequest.FamilyMemberId);
        
        var result = await _documentReppository.AddAsync(_mapper.Map<DocumentDetails>(createDocumentRequest));

        _logger.LogInformation("Document created with Id: {DocumentId}", result.Id);

        return _mapper.Map<DocumentDetailsDto>(result);        
    }

    public async Task DeleteDocumentDetailsByIdAsync(Guid documentId)
    {
        _logger.LogInformation("Deleting document with Id: {DocumentId}", documentId);

        await _documentReppository.DeleteByIdAsync(documentId,"Harshil");

        _logger.LogInformation("Document with Id: {DocumentId} deleted successfully", documentId);
    }

    public async Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsAsync()
    {
        var result = await _documentReppository.GetAllAsync();
        return _mapper.Map<List<DocumentDetailsDto>>(result);
    }

    public async Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId)
    {
        var result = await _documentReppository.GetAsyncbyId(documentId);
        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task<DocumentDetailsDto> UpdateDocumentDetailsAsync(UpdateDocumentRequest updateDocumentRequest)
    {
        _logger.LogInformation("Updating document with Id: {DocumentId}", updateDocumentRequest.Id);
        
        var result = await _documentReppository.AddAsync(_mapper.Map<DocumentDetails>(updateDocumentRequest));
       
        _logger.LogInformation("Document with Id: {DocumentId} updated successfully", result.Id);

        return _mapper.Map<DocumentDetailsDto>(result);
    }
}
