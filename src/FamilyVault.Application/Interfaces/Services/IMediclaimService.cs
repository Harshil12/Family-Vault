using FamilyVault.Application.DTOs.Mediclaim;

namespace FamilyVault.Application.Interfaces.Services;

public interface IMediclaimService
{
    Task<MediclaimPolicyDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<MediclaimPolicyDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<MediclaimPolicyDetailsDto> CreateAsync(CreateMediclaimPolicyRequest request, Guid userId, CancellationToken cancellationToken);
    Task<MediclaimPolicyDetailsDto> UpdateAsync(UpdateMediclaimPolicyRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
