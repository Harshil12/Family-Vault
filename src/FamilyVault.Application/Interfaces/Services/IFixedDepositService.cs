using FamilyVault.Application.DTOs.FixedDeposits;

namespace FamilyVault.Application.Interfaces.Services;

public interface IFixedDepositService
{
    Task<FixedDepositDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<FixedDepositDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<FixedDepositDetailsDto> CreateAsync(CreateFixedDepositRequest request, Guid userId, CancellationToken cancellationToken);
    Task<FixedDepositDetailsDto> UpdateAsync(UpdateFixedDepositRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
