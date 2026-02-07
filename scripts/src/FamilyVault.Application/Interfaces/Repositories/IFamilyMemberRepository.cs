using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IFamilyMemberRepository : IGenericRepository<FamilyMember>
{
    Task<FamilyMember?> GetByIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FamilyMember>> GetAllWithDocumentsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FamilyMember>> GetAllByFamilyIdAsync(Guid FamilyId, CancellationToken cancellationToken);
    new Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
