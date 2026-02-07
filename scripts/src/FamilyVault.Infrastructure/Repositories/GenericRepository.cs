using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace FamilyVault.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _appDbContext;
    protected readonly IMemoryCache _memoryCache;
    private readonly string _cacheKeyPrefix;

    public GenericRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _appDbContext = appDbContext;
        _memoryCache = memoryCache;
        _cacheKeyPrefix = typeof(T).Name;
    }

    protected virtual MemoryCacheEntryOptions DefaultCacheOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(2),
        Priority = CacheItemPriority.High
    };

    protected string BuildCacheKey(string suffix) => $""{_cacheKeyPrefix}:{suffix}"";

    private string CacheEvictionTokenKey => $""{_cacheKeyPrefix}:_EvictionToken"";

    protected CancellationTokenSource GetOrCreateEvictionToken()
    {
        if (!_memoryCache.TryGetValue(CacheEvictionTokenKey, out CancellationTokenSource? cts) || cts is null)
        {
            cts = new CancellationTokenSource();
            _memoryCache.Set(CacheEvictionTokenKey, cts);
        }
        return cts;
    }

    protected void InvalidateCache()
    {
        if (_memoryCache.TryGetValue(CacheEvictionTokenKey, out CancellationTokenSource? oldCts) && oldCts is not null)
        {
            try { oldCts.Cancel(); } catch { }
            try { oldCts.Dispose(); } catch { }
        }

        var newCts = new CancellationTokenSource();
        _memoryCache.Set(CacheEvictionTokenKey, newCts);
    }

    protected async Task<TItem> GetOrCreateCachedAsync<TItem>(string keySuffix, Func<Task<TItem>> factory, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(keySuffix);
        var evictionToken = GetOrCreateEvictionToken();

        return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetOptions(DefaultCacheOptions);
            entry.AddExpirationToken(new CancellationChangeToken(evictionToken.Token));
            return await factory();
        });
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _appDbContext.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await GetOrCreateCachedAsync(""All"", async () =>
        {
            return await _appDbContext.Set<T>().AsNoTracking().ToListAsync(cancellationToken);
        }, cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _appDbContext.Set<T>().AddAsync(entity, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == entity.Id, cancellationToken)
            ?? throw new KeyNotFoundException($""{typeof(T).Name} not found"");

        _appDbContext.Entry(existing).CurrentValues.SetValues(entity);

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
        return entity;
    }

    public virtual async Task DeleteByIdAsync(Guid id, string user, CancellationToken cancellationToken)
    {
        var existing = await _appDbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($""{typeof(T).Name} not found"");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        existing.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        InvalidateCache();
    }
}
