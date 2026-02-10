using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
        : base(appDbContext, memoryCache)
    {
    }

    public async Task<IReadOnlyList<User>> GetAllWithFamilyDetailsAsync(CancellationToken cancellationToken)
    {
        // cache suffix: WithFamilies
        return await GetOrCreateCachedAsync("WithFamilies", async () =>
        {
            return await _appDbContext.Users
                .Where(u => !u.IsDeleted)
                .AsNoTracking()
                .Include(u => u.Families.Where(f => !f.IsDeleted))
                .ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _appDbContext.Users
           .Where(u => !u.IsDeleted)
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}

