using FamilyVault.Application.DTOs.Family;

namespace FamilyVault.Application.Interfaces.Services;

/// <summary>
/// Defines methods for managing family records, including retrieval, creation, updating, and deletion operations.
/// </summary>
/// <remarks>Implementations of this interface should ensure thread safety if accessed concurrently. Methods are
/// asynchronous and return tasks that complete when the corresponding operation finishes.</remarks>
public interface IFamilyService
{
    /// <summary>
    /// Asynchronously retrieves the family record with the specified unique identifier.
    /// </summary>
    /// <param name="familyId">The unique identifier of the family to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="FamilyDto"/>
    /// representing the family if found; otherwise, <c>null</c>.</returns>
    public Task<FamilyDto> GetFamilyByIdAsync(Guid familyId);

    /// <summary>
    /// Asynchronously retrieves a read-only list of families.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of <see
    /// cref="FamilyDto"/> objects representing the families. The list is empty if no families are found.</returns>
    public Task<IReadOnlyList<FamilyDto>> GetFamilyAsync();

    /// <summary>
    /// Updates the details of an existing family asynchronously using the specified request data.
    /// </summary>
    /// <param name="createFamlyRequest">The request containing the updated family information. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="FamilyDto"/> with the
    /// updated family details.</returns>
    public Task<FamilyDto> UpdateFamilyAsync(CreateFamlyRequest createFamlyRequest);

    /// <summary>
    /// Creates a new family record asynchronously using the specified update request.
    /// </summary>
    /// <param name="updateFamlyRequest">An object containing the details required to create the family. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="FamilyDto"/>
    /// representing the newly created family.</returns>
    public Task<FamilyDto> CreateFamilyAsync(UpdateFamlyRequest updateFamlyRequest);

    /// <summary>
    /// Deletes the family with the specified unique identifier asynchronously.
    /// </summary>
    /// <param name="familyId">The unique identifier of the family to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public Task DeleteFamilyByIdAsync(Guid familyId);
}

