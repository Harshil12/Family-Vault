using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

internal class DocumentRepository(AppDbContext appDbContext) : IDocumentRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<IReadOnlyList<DocumentDetails>> GetAllAsync()
    {
        return await _appDbContext.Documents.AsNoTracking().ToListAsync();
    }

    public async Task<DocumentDetails?> GetAsyncbyId(Guid documentId)
    {
        return await _appDbContext.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<DocumentDetails> AddAsync(DocumentDetails documentDetails)
    {
        _appDbContext.Documents.Add(documentDetails);
        await _appDbContext.SaveChangesAsync();
        return documentDetails;
    }

    public async Task<DocumentDetails> UpdateAsync(DocumentDetails documentDetails)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentDetails.Id) ?? throw new KeyNotFoundException("Document not found");

        existingDocument.DocumentType = documentDetails.DocumentType;
        existingDocument.DocumentNumber = documentDetails.DocumentNumber;
        existingDocument.IssueDate = documentDetails.IssueDate;
        existingDocument.ExpiryDate = documentDetails.ExpiryDate;
        existingDocument.SavedLocation = documentDetails.SavedLocation;
    
        await _appDbContext.SaveChangesAsync();
        return documentDetails;
    }

    public async Task DeleteByIdAsync(Guid documentId, string user)
    {
        var existingDocument = await _appDbContext.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId) ?? throw new KeyNotFoundException("Document not found");

        existingDocument.IsDeleted = true;
        existingDocument.UpdatedAt = DateTimeOffset.UtcNow;
        existingDocument.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync();
    }

}
