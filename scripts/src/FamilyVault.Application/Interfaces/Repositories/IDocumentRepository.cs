using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IDocumentRepository : IGenericRepository<DocumentDetails>
{
    Task<DocumentDetails?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<DocumentDetails>> GetAllByFamilyMemberIdAsync(Guid FamilyMemberId, CancellationToken cancellationToken);
}
