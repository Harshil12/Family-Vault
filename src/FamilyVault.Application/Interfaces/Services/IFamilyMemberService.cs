using FamilyVault.Application.DTOs.FamilyMembers;

namespace FamilyVault.Application.Interfaces.Services;

/// <summary>
/// Defines the contract for managing family member records, including operations to create, retrieve, update, and
/// delete family members asynchronously.
/// </summary>
/// <remarks>Implementations of this interface should ensure thread safety for concurrent operations and validate
/// input parameters according to method requirements. All methods are asynchronous and return tasks that complete when
/// the corresponding operation finishes.</remarks>
public interface IFamilyMemberService
{
    /// <summary>
    /// Asynchronously retrieves a family member by their unique identifier.
    /// </summary>
    /// <param name="familyMemberId">The unique identifier of the family member to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="FamilyMemberDto"/>
    /// representing the family member if found; otherwise, <see langword="null"/>.</returns>
    public Task<FamilyMemberDto> GetFamilyMemberByIdAsync(Guid familyMemberId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a read-only list of family members.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of <see
    /// cref="FamilyMemberDto"/> objects representing the family members. The list is empty if no family members are
    /// found.</returns>
    public Task<IReadOnlyList<FamilyMemberDto>> GetFamilyMembersByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the details of an existing family member asynchronously using the specified request data.
    /// </summary>
    /// <param name="updateFamilyMemberRequest">The request containing the updated information for the family member. Must not be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="FamilyMemberDto"/> with
    /// the updated family member details.</returns>
    public Task<FamilyMemberDto> UpdateFamilyMemberAsync(UpdateFamilyMemberRequest updateFamilyMemberRequest, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new family member using the specified update request.
    /// </summary>
    /// <param name="createFamilyMemberRequest">An object containing the details of the family member to create. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="FamilyMemberDto"/>
    /// representing the created family member.</returns>
    public Task<FamilyMemberDto> CreateFamilyMemberAsync(CreateFamilyMemberRequest createFamilyMemberRequest, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deletes the family member with the specified unique identifier.
    /// </summary>
    /// <param name="familyMemberId">The unique identifier of the family member to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public Task DeleteFamilyMemberByIdAsync(Guid familyMemberId, Guid userId, CancellationToken cancellationToken);
}

