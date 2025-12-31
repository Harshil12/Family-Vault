using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Services;

public class Userservice : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public Userservice(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    public async Task<UserDto> CreateUserAsync(CreateUserRequest createUserRequest)
    {
        var result = await  _userRepository.AddAsync(_mapper.Map<Domain.Entities.User>(createUserRequest));
        return _mapper.Map<UserDto>(result);
    }

    public async Task DeleteUserByIdAsync(Guid userId)
    {
        await _userRepository.DeleteByIdAsync(userId, "Harshil");
    }

    public async Task<IReadOnlyList<UserDto>> GetUserAsync()
    {
        var users = await _userRepository.GetAllAsync();    
        return _mapper.Map<IReadOnlyList<UserDto>>(users);
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateuUerAsync(UpdateUserRequest updateUserRequest)
    {
        var user = await _userRepository.UpdateAsync(_mapper.Map<User>(updateUserRequest));
        return _mapper.Map<UserDto>(user);
    }
}
