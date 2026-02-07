using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class DocumentService : GenericService<DocumentDetailsDto, DocumentDetails>, IDocumentService
{
    private readonly ICryptoService _cryptoService;
    private readonly IDocumentRepository _documentReppository;
    private readonly ILogger<DocumentService> _typedLogger;

    public DocumentService(IDocumentRepository documentRepository, ICryptoService cryptoService, IMapper mapper, ILogger<DocumentService> logger)
        : base(documentRepository, mapper, logger)
    {
        _cryptoService = cryptoService;
        _documentReppository = documentRepository;
        _typedLogger = logger;
    }

    public async Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);

        foreach (var doc in result)
        {
            doc.DocumentNumber = _crypto_service.DecryptData(doc.DocumentNumber);
        }

        return _mapper.Map<List<DocumentDetailsDto>>(result);
    }

    public async Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentReppository.GetByIdAsync(documentId, cancellationToken);

        if (result != null)
            result.DocumentNumber = _crypto_service.DecryptData(result.DocumentNumber);

        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Creating a new document for FamilyMemberId: {FamilyMemberId}\", createDocumentRequest.FamilyMemberId);

        var documentToCreate = _mapper.Map<DocumentDetails>(createDocumentRequest);
        documentToCreate.DocumentNumber = _crypto_service.EncryptData(documentToCreate.DocumentNumber);

        return await CreateAsync(documentToCreate, userId, cancellationToken);
    }

    public Task DeleteDocumentDetailsByIdAsync(Guid documentId, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting document with Id: {DocumentId}\", documentId);
        return DeleteAsync(documentId, userId, cancellationToken);
    }

    public async Task<DocumentDetailsDto> UpdateDocumentDetailsAsync(UpdateDocumentRequest updateDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating document with Id: {DocumentId}\", updateDocumentRequest.Id);

        var documentToUpdate = _mapper.Map<DocumentDetails>(updateDocumentRequest);
        documentToUpdate.DocumentNumber = _crypto_service.EncryptData(documentToUpdate.DocumentNumber);

        return await UpdateAsync(documentToUpdate, userId, cancellationToken);
    }
}
