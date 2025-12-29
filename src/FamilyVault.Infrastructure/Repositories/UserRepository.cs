using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    public Task<User> CreateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUserByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<User>> GetUserAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUserByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<User> UpdateUserAsync(User user)
    {
        throw new NotImplementedException();
    }
}
