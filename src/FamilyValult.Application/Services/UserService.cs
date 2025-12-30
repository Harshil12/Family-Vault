using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;

namespace FamilyVault.Application.Services;

public class Userservice : IUserService
{
    private readonly IUserRepository _userRepository;

    public Userservice(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public Task<UserDto> CreateUserAsync(UpdateUserRequest updateUserRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUserByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<UserDto>> GetUserAsync()
    {
        throw new NotImplementedException();
    }

    public Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<UserDto> UpdateuUerAsync(CreateUserRequest createUserRequest)
    {
        throw new NotImplementedException();
    }
}
