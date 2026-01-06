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

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        return await _appDbContext.Users.AsNoTracking().ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _appDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User> AddAsync(User user)
    {
        _appDbContext.Add(user);
        await _appDbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        var existingUser = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id)
     ?? throw new KeyNotFoundException("User not found");

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.CountryCode = user.CountryCode;
        existingUser.Mobile = user.Mobile;
        existingUser.Username = user.Username;
        existingUser.Password = user.Password;
   
        await _appDbContext.SaveChangesAsync();
        return existingUser;
    }
    public async Task DeleteByIdAsync(Guid userId, string user)
    {
        var existingUser = await _appDbContext.Users
           .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("User not found");

        existingUser.IsDeleted = true;
        existingUser.UpdatedAt = DateTimeOffset.UtcNow;
        existingUser.UpdatedBy = user;

        await _appDbContext.SaveChangesAsync();
    }
}
