using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyVault.Infrastructure.Repositories;

/// <summary>
/// Represents BankAccountRepository.
/// </summary>
public class BankAccountRepository : IBankAccountRepository
{
    private readonly AppDbContext _appDbContext;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of BankAccountRepository.
    /// </summary>
    public BankAccountRepository(AppDbContext appDbContext, IMemoryCache memoryCache)
    {
        _appDbContext = appDbContext;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Performs the GetByIdAsync operation.
    /// </summary>
    public async Task<BankAccountDetails?> GetByIdAsync(Guid bankAccountId, CancellationToken cancellationToken)
    {
        return await _appDbContext.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bankAccountId, cancellationToken);
    }

    /// <summary>
    /// Performs the GetAllByFamilyMemberIdAsync operation.
    /// </summary>
    public async Task<IReadOnlyList<BankAccountDetails>> GetAllByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.High
        };

        var cacheKey = $"BankAccounts:{familyMemberId}";
        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<BankAccountDetails>? cachedAccounts) && cachedAccounts is not null)
        {
            return cachedAccounts;
        }

        var result = await _appDbContext.BankAccounts
            .Where(b => b.FamilyMemberId == familyMemberId && !b.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        _memoryCache.Set(cacheKey, result, cacheOptions);
        return result;
    }

    /// <summary>
    /// Performs the AddAsync operation.
    /// </summary>
    public async Task<BankAccountDetails> AddAsync(BankAccountDetails bankAccount, CancellationToken cancellationToken)
    {
        await _appDbContext.BankAccounts.AddAsync(bankAccount, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove($"BankAccounts:{bankAccount.FamilyMemberId}");

        return bankAccount;
    }

    /// <summary>
    /// Performs the UpdateAsync operation.
    /// </summary>
    public async Task<BankAccountDetails> UpdateAsync(BankAccountDetails bankAccount, CancellationToken cancellationToken)
    {
        var existingBankAccount = await _appDbContext.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == bankAccount.Id, cancellationToken) ?? throw new KeyNotFoundException("Bank account not found");

        existingBankAccount.BankName = bankAccount.BankName;
        existingBankAccount.AccountNumber = bankAccount.AccountNumber;
        existingBankAccount.AccountNumberLast4 = bankAccount.AccountNumberLast4;
        existingBankAccount.AccountType = bankAccount.AccountType;
        existingBankAccount.AccountHolderName = bankAccount.AccountHolderName;
        existingBankAccount.IFSC = bankAccount.IFSC;
        existingBankAccount.Branch = bankAccount.Branch;
        existingBankAccount.NomineeName = bankAccount.NomineeName;
        existingBankAccount.FamilyMemberId = bankAccount.FamilyMemberId;
        existingBankAccount.UpdatedAt = bankAccount.UpdatedAt;
        existingBankAccount.UpdatedBy = bankAccount.UpdatedBy;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove($"BankAccounts:{existingBankAccount.FamilyMemberId}");

        return existingBankAccount;
    }

    /// <summary>
    /// Performs the DeleteByIdAsync operation.
    /// </summary>
    public async Task DeleteByIdAsync(Guid bankAccountId, string user, CancellationToken cancellationToken)
    {
        var existingBankAccount = await _appDbContext.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == bankAccountId, cancellationToken) ?? throw new KeyNotFoundException("Bank account not found");

        existingBankAccount.IsDeleted = true;
        existingBankAccount.UpdatedAt = DateTimeOffset.UtcNow;
        existingBankAccount.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove($"BankAccounts:{existingBankAccount.FamilyMemberId}");
    }
}
