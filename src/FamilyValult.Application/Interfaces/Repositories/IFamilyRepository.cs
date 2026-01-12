using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories;

/// <summary>
/// Defines a contract for managing family entities in the repository.
/// Provides methods to create, read, update, and delete family records.
/// </summary>
public interface IFamilyRepository
{
    /// <summary>
    /// Retrieves a family by its unique identifier.
    /// </summary>
    /// <param name="familyId">The unique identifier of the family.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the <see cref="Family"/> entity if found.
    /// </returns>
    public Task<Family?> GetByIdAsync(Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all families from the repository.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains a read-only list of <see cref="Family"/> entities.
    /// </returns>
    public Task<IReadOnlyList<Family>> GetAllWithFamilyMembersAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all families from the repository.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains a read-only list of <see cref="Family"/> entities.
    /// </returns>
    public Task<IReadOnlyList<Family>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new family record in the repository.
    /// </summary>
    /// <param name="family">The family entity to create.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the newly created <see cref="Family"/> entity.
    /// </returns>
    public Task<Family> AddAsync(Family family, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the details of an existing family.
    /// </summary>
    /// <param name="family">The family entity with updated details.</param>
    /// <returns>
    /// A task representing the asynchronous operation. 
    /// The task result contains the updated <see cref="Family"/> entity.
    /// </returns>
    public Task<Family> UpdateAsync(Family family, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a family from the repository using its unique identifier.
    /// </summary>
    /// <param name="familyId">The unique identifier of the family to delete.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    public Task DeleteByIdAsync(Guid familyId, string user, CancellationToken cancellationToken);
}