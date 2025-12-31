using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Interfaces.Repositories
{
    /// <summary>
    /// Defines a contract for managing family member entities in the repository.
    /// Provides methods to create, read, update, and delete family member records.
    /// </summary>
    public interface IFamilyMemberRepository
    {
        /// <summary>
        /// Retrieves a family member by their unique identifier.
        /// </summary>
        /// <param name="familyMemberId">The unique identifier of the family member.</param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The task result contains the <see cref="FamilyMember"/> entity if found.
        /// </returns>
        public Task<FamilyMember?> GetByIdAsync(Guid familyMemberId);

        /// <summary>
        /// Retrieves all family members from the repository.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The task result contains a read-only list of <see cref="FamilyMember"/> entities.
        /// </returns>
        public Task<IReadOnlyList<FamilyMember>> GetAllAsync();

        /// <summary>
        /// Creates a new family member record in the repository.
        /// </summary>
        /// <param name="familyMember">The family member entity to create.</param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The task result contains the newly created <see cref="FamilyMember"/> entity.
        /// </returns>
        public Task<FamilyMember> AddAsync(FamilyMember familyMember);

        /// <summary>
        /// Updates the details of an existing family member.
        /// </summary>
        /// <param name="familyMember">The family member entity with updated details.</param>
        /// <returns>
        /// A task representing the asynchronous operation. 
        /// The task result contains the updated <see cref="FamilyMember"/> entity.
        /// </returns>
        public Task<FamilyMember> UpdateAsync(FamilyMember familyMember);
                
        /// <summary>
        /// Deletes a family member from the repository using their unique identifier.
        /// </summary>
        /// <param name="familyMemberId">The unique identifier of the family member to delete.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        public Task DeleteByIdAsync(Guid familyMemberId, string user);
    }
}