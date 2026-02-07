using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<IReadOnlyList<User>> GetAllWithFamilyDetailsAsync(CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}
