using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

/// <summary>
/// Defines a contract for managing user entities in the repository.
/// Provides methods to create, read, update, and delete user records.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the <see cref="User"/> entity if found.
    /// </returns>
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all users from the repository.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains a read-only list of <see cref="User"/> entities.
    /// </returns>
    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new user record in the repository.
    /// </summary>
    /// <param name="user">The user entity to create.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the newly created <see cref="User"/> entity.
    /// </returns>
    public Task<User> AddAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the details of an existing user.
    /// </summary>
    /// <param name="user">The user entity with updated details.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the updated <see cref="User"/> entity.
    /// </returns>
    public Task<User> UpdateAsync(User user, CancellationToken cancellationToken);       

    /// <summary>
    /// Deletes a user from the repository using their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public Task DeleteByIdAsync(Guid userId, string user, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a user by their email.
    /// </summary>
    /// <param name="userId">The email id of the user.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the <see cref="User"/> entity if found.
    /// </returns>
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}