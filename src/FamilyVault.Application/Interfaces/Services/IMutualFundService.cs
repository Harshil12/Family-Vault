using FamilyVault.Application.DTOs.MutualFunds;

namespace FamilyVault.Application.Interfaces.Services;

public interface IMutualFundService
{
    Task<MutualFundHoldingDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<MutualFundHoldingDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<MutualFundHoldingDetailsDto> CreateAsync(CreateMutualFundHoldingRequest request, Guid userId, CancellationToken cancellationToken);
    Task<MutualFundHoldingDetailsDto> UpdateAsync(UpdateMutualFundHoldingRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
