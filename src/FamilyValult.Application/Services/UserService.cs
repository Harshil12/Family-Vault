using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class Userservice : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public Userservice(IUserRepository userRepository, IMapper mapper, ILogger<Userservice> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<UserDto> CreateUserAsync(CreateUserRequest createUserRequest)
    {
        _logger.LogInformation("Creating a new user with username: {Username}", createUserRequest.Username);

        var userToCreate = _mapper.Map<User>(createUserRequest);
        userToCreate.CreatedAt = DateTimeOffset.Now;
        userToCreate.CreatedBy = "Harshil";

        var result = await _userRepository.AddAsync(userToCreate);

        _logger.LogInformation("User created successfully with ID: {UserId}", result.Id);

        return _mapper.Map<UserDto>(result);
    }

    public async Task DeleteUserByIdAsync(Guid userId)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", userId);

        await _userRepository.DeleteByIdAsync(userId, "Harshil");
    
        _logger.LogInformation("User with ID: {UserId} deleted successfully", userId);
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
        _logger.LogInformation("Updating user with ID: {UserId}", updateUserRequest.Id);
       
        var userToUpdate = _mapper.Map<User>(updateUserRequest);
        userToUpdate.UpdatedAt = DateTimeOffset.Now;
        userToUpdate.UpdatedBy = "Harshil";

        var user = await _userRepository.UpdateAsync(userToUpdate);

        _logger.LogInformation("User with ID: {UserId} updated successfully", updateUserRequest.Id);
        
        return _mapper.Map<UserDto>(user);
    }
}
