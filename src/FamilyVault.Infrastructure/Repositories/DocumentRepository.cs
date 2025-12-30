using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

internal class DocumentRepository(AppDbContext appDbContext) : IDocumentRepository
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<IReadOnlyList<DocumentDetails>> GetDocumentsDetailsAsync()
    {
        return await _appDbContext.Documents.AsNoTracking().ToListAsync();
    }

    public async Task<DocumentDetails?> GetDocumentsDetailsByIdAsync(Guid documentId)
    {
        return await _appDbContext.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<DocumentDetails> CreateDocumentsDetailsAsync(DocumentDetails documentDetails)
    {
        _appDbContext.Documents.Add(documentDetails);
        await _appDbContext.SaveChangesAsync();
        return documentDetails;
    }

    public async Task<DocumentDetails> UpdateDocumentsDetailsAsync(DocumentDetails documentDetails)
    {
        _appDbContext.Documents.Update(documentDetails);
        await _appDbContext.SaveChangesAsync();
        return documentDetails;
    }

    public async Task DeleteDocumentsDetailsByIdAsync(Guid documentId)
    {
       _appDbContext.Documents.Remove(
            await _appDbContext.Documents
                .FirstOrDefaultAsync(d => d.Id == documentId) ?? 
            throw new InvalidOperationException("Document not found")); 
    }

    
}
