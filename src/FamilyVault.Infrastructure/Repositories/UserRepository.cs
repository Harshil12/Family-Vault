using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Domain.Entities;
using FamilyVault.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyVault.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _appDbContext;

    public UserRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _appDbContext.Users.AsNoTracking().ToListAsync(cancellationToken);
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

        return existingUser;
    }
    public async Task DeleteByIdAsync(Guid userId, string user, CancellationToken cancellationToken)
    {
        var existingUser = await _appDbContext.Users
           .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken) ?? throw new KeyNotFoundException("User not found");

        existingUser.IsDeleted = true;
        existingUser.UpdatedAt = DateTimeOffset.UtcNow;
        existingUser.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync(cancellationToken);
    }
}
