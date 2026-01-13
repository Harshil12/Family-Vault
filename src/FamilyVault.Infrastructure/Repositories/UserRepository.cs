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
        var existingUser = await _appDbContext.Users
           .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken) ?? throw new KeyNotFoundException("User not found");

        existingUser.IsDeleted = true;
        existingUser.UpdatedAt = DateTimeOffset.UtcNow;
        existingUser.UpdatedBy = user;

        _memoryCache.Remove("UsersWithFamilies");
        _memoryCache.Remove("UsersFamilies");

        await _appDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _appDbContext.Users
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
