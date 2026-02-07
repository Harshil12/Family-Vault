using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class UserService : GenericService<UserDto, User>, IUserService
{
    private readonly ICryptoService _cryptoService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _typedLogger;

    public UserService(IUserRepository userRepository, ICryptoService cryptoService, IMapper mapper, ILogger<UserService> logger)
        : base(userRepository, mapper, logger)
    {
        _cryptoService = cryptoService;
        _userRepository = userRepository;
        _typedLogger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> GetUserAsync(CancellationToken cancellationToken)
        => await GetAllAsync(cancellationToken);

    public Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
        => GetByIdAsync(userId, cancellationToken);

    public async Task<UserDto> CreateUserAsync(CreateUserRequest createUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Creating a new user\");

        var userToCreate = _mapper.Map<User>(createUserRequest);
        userToCreate.Password = _cryptoService.HashPassword(createUserRequest.Password);

        return await CreateAsync(userToCreate, createdByUserId, cancellationToken);
    }

    public Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting user with ID: {UserId}\", userId);
        return DeleteAsync(userId, createdByUserId, cancellationToken);
    }

    public async Task<UserDto> UpdateUserAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating user with ID: {UserId}\", updateUserRequest.Id);

        var userToUpdate = _mapper.Map<User>(updateUserRequest);

        // only hash and update password if provided in request
        if (!string.IsNullOrEmpty(updateUserRequest.Password))
        {
            userToUpdate.Password = _cryptoService.HashPassword(updateUserRequest.Password);
        }

        return await UpdateAsync(userToUpdate, createdByUserId, cancellationToken);
    }
}
