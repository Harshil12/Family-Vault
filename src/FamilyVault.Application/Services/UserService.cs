using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class Userservice : IUserService
{
    private readonly ICryptoService _cryptoService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public Userservice(IUserRepository userRepository, ICryptoService cryptoService, IMapper mapper, ILogger<Userservice> logger)
    {
        _cryptoService = cryptoService;
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<IReadOnlyList<UserDto>> GetUserAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<UserDto>>(users);
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest createUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new user with username: {Username}", createUserRequest.Username);

        var userToCreate = _mapper.Map<User>(createUserRequest);

        userToCreate.Password = _cryptoService.HashPassword(createUserRequest.Password);
        userToCreate.CreatedAt = DateTimeOffset.Now;
        userToCreate.CreatedBy = createdByUserId.ToString();

        var result = await _userRepository.AddAsync(userToCreate, cancellationToken);

        _logger.LogInformation("User created successfully with ID: {UserId}", result.Id);

        return _mapper.Map<UserDto>(result);
    }

    public async Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", userId);

        await _userRepository.DeleteByIdAsync(userId, createdByUserId.ToString(), cancellationToken);

        _logger.LogInformation("User with ID: {UserId} deleted successfully", userId);
    }

    public async Task<UserDto> UpdateuUerAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", updateUserRequest.Id);

        var userToUpdate = _mapper.Map<User>(updateUserRequest);

        userToUpdate.Password = _cryptoService.HashPassword(userToUpdate.Password);
        userToUpdate.UpdatedAt = DateTimeOffset.Now;
        userToUpdate.UpdatedBy = createdByUserId.ToString();

        var user = await _userRepository.UpdateAsync(userToUpdate, cancellationToken);

        _logger.LogInformation("User with ID: {UserId} updated successfully", updateUserRequest.Id);

        return _mapper.Map<UserDto>(user);
    }
}
