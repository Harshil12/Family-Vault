using FamilyVault.Application.DTOs.User;

namespace FamilyVault.Application.Interfaces.Services;

public interface IUserService
{
    public Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    public Task<IReadOnlyList<UserDto>> GetUserAsync(CancellationToken cancellationToken);
    public Task<UserDto> UpdateUserAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken);
    public Task<UserDto> CreateUserAsync(CreateUserRequest createUserRequest, Guid createdByUserId, CancellationToken cancellationToken);
    public Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken);
}
