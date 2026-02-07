using AutoMapper;
using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class DocumentService : GenericService<DocumentDetailsDto, DocumentDetails>, IDocumentService
{
    private readonly ICryptoService _cryptoService;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<DocumentService> _typedLogger;

    public DocumentService(IDocumentRepository documentRepository, ICryptoService cryptoService, IMapper mapper, ILogger<DocumentService> logger)
        : base(documentRepository, mapper, logger)
    {
        _cryptoService = cryptoService;
        _documentRepository = documentRepository;
        _typedLogger = logger;
    }

    public async Task<IReadOnlyList<DocumentDetailsDto>> GetDocumentsDetailsByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var result = await _documentRepository.GetAllByFamilyMemberIdAsync(familyMemberId, cancellationToken);

        // map to DTOs (do not mutate entities)
        var dtos = _mapper.Map<List<DocumentDetailsDto>>(result);
        foreach (var dto in dtos)
        {
            dto.DocumentNumber = _cryptoService.DecryptData(dto.DocumentNumber);
        }

        return dtos;
    }

    public async Task<DocumentDetailsDto> GetDocumentDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (result != null)
        {
            var dto = _mapper.Map<DocumentDetailsDto>(result);
            dto.DocumentNumber = _cryptoService.DecryptData(dto.DocumentNumber);
            return dto;
        }

        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest, Guid userId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Creating a new document for FamilyMemberId: {FamilyMemberId}\", createDocumentRequest.FamilyMemberId);

        var documentToCreate = _mapper.Map<DocumentDetails>(createDocumentRequest);
        documentToCreate.DocumentNumber = _cryptoService.EncryptData(documentToCreate.DocumentNumber);

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
        documentToUpdate.DocumentNumber = _cryptoService.EncryptData(documentToUpdate.DocumentNumber);

        return await UpdateAsync(documentToUpdate, userId, cancellationToken);
    }
}
