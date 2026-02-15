using FamilyVault.Application.DTOs.DematAccounts;

namespace FamilyVault.Application.Interfaces.Services;

public interface IDematAccountService
{
    Task<DematAccountDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<DematAccountDetailsDto>> GetByFamilyMemberIdAsync(Guid familyMemberId, CancellationToken cancellationToken);
    Task<DematAccountDetailsDto> CreateAsync(CreateDematAccountRequest request, Guid userId, CancellationToken cancellationToken);
    Task<DematAccountDetailsDto> UpdateAsync(UpdateDematAccountRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
