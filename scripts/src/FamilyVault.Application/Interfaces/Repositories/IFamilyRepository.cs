using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IFamilyRepository : IGenericRepository<Family>
{
    Task<IReadOnlyList<Family>> GetAllWithFamilyMembersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Family>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    new Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken);
}
