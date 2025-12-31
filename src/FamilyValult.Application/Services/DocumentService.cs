using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using AutoMapper;

namespace FamilyVault.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentReppository;
    private readonly IMapper _mapper;

    public DocumentService(IDocumentRepository documentRepository, IMapper mapper)
    {
        _documentReppository = documentRepository;
        _mapper = mapper;
    }
    public async Task<DocumentDetailsDto> CreateDocumentDetailsAsync(CreateDocumentRequest createDocumentRequest)
    {
        var result = await _documentReppository.AddAsync(_mapper.Map<DocumentDetails>(createDocumentRequest));
        return _mapper.Map<DocumentDetailsDto>(result);
    }

    public async Task DeleteDocumentDetailsByIdAsync(Guid documentId)
    {
        await _documentReppository.DeleteByIdAsync(documentId,"Harshil");
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
        var result = await _documentReppository.AddAsync(_mapper.Map<DocumentDetails>(updateDocumentRequest));
        return _mapper.Map<DocumentDetailsDto>(result);
    }
}
