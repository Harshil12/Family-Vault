using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _memoryCache;

    public UserRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _appDbContext = appDbContext;
        _memoryCache = memoryCache;
    }

    public async Task<IReadOnlyList<User>> GetAllWithFamilyDetailsAsync(CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue("UsersWithFamilies", out IReadOnlyList<User>? cachedUsers) && cachedUsers is not null)
        {
            return cachedUsers;
        }

        var result = await _appDbContext.Users.AsNoTracking().Include(f => f.Families).ToListAsync(cancellationToken);
        _memoryCache.Set("UsersWithFamilies", result, cacheOptions);
        return result;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        if (_memoryCache.TryGetValue("UsersFamilies", out IReadOnlyList<User>? cachedUsers) && cachedUsers is not null)
        {
            return cachedUsers;
        }
        var result = await _appDbContext.Users.AsNoTracking().ToListAsync(cancellationToken);

        _memoryCache.Set("UsersFamilies", result, cacheOptions);

        return result;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _appDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken)
    {
        await _appDbContext.AddAsync(user, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("UsersWithFamilies");
        _memoryCache.Remove("UsersFamilies");

        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken)
    {
        var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken)
     ?? throw new KeyNotFoundException("User not found");

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.CountryCode = user.CountryCode;
        existingUser.Mobile = user.Mobile;
        existingUser.Username = user.Username;
        existingUser.Password = user.Password;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove("UsersWithFamilies");
        _memoryCache.Remove("UsersFamilies");

        return existingUser;
    }
    public async Task DeleteByIdAsync(Guid userId, string user, CancellationToken cancellationToken)
    {
        // Soft delete the user and related families and family members and documents
        await _appDbContext.Users
               .Where(fm => fm.Id == userId)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await _appDbContext.Families
               .Where(fm => fm.UserId == userId)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await _appDbContext.FamilyMembers
               .Where(fm => _appDbContext.Families
                   .Where(f => f.UserId == userId && !f.IsDeleted)
                   .Select(f => f.Id)
                   .Contains(fm.FamilyId)
               && !fm.IsDeleted)
               .ExecuteUpdateAsync(setter => setter.SetProperty(fm => fm.IsDeleted, true)
               .SetProperty(fm => fm.UpdatedBy, user)
               .SetProperty(fm => fm.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken: cancellationToken);

        await (
                from d in _appDbContext.Documents
                join m in _appDbContext.FamilyMembers on d.FamilyMemberId equals m.Id
                join f in _appDbContext.Families on m.FamilyId equals f.Id
                where f.UserId == userId
                      && !f.IsDeleted
                      && !m.IsDeleted
                      && !d.IsDeleted
                select d
              )
              .ExecuteUpdateAsync(setter =>
                setter.SetProperty(d => d.IsDeleted, true)
                      .SetProperty(d => d.UpdatedBy, user)
                      .SetProperty(d => d.UpdatedAt, DateTimeOffset.UtcNow),
                cancellationToken);

        _memoryCache.Remove("UsersWithFamilies");
        _memoryCache.Remove("UsersFamilies");
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _appDbContext.Users
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
