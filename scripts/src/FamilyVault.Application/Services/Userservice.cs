using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Interfaces.Repositories;
using FamilyVault.Application.Interfaces.Services;
using FamilyVault.Application.Services;
using FamilyVault.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FamilyVault.Application.Services;

public class Userservice : GenericService<UserDto, User>, IUserService
{
    private readonly ICryptoService _cryptoService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<Userservice> _typedLogger;

    public Userservice(IUserRepository userRepository, ICryptoService cryptoService, IMapper mapper, ILogger<Userservice> logger)
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
        _typedLogger.LogInformation(\"Creating a new user with username: {Username}\", createUserRequest.Username);

        var userToCreate = _mapper.Map<User>(createUserRequest);
        userToCreate.Password = _cryptoService.HashPassword(createUserRequest.Password);

        return await CreateAsync(userToCreate, createdByUserId, cancellationToken);
    }

    public Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Deleting user with ID: {UserId}\", userId);
        return DeleteAsync(userId, createdByUserId, cancellationToken);
    }

    public async Task<UserDto> UpdateuUerAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken)
    {
        _typedLogger.LogInformation(\"Updating user with ID: {UserId}\", updateUserRequest.Id);

        var userToUpdate = _mapper.Map<User>(updateUserRequest);
        userToUpdate.Password = _cryptoService.HashPassword(userToUpdate.Password);

        return await UpdateAsync(userToUpdate, createdByUserId, cancellationToken);
    }
}
